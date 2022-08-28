using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using BlazorBindGen.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BlazorBindGenerator
{
    [Generator]
    public class BindGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not BindingSyntaxReciever reciever)
                return;
            nameCount = 0;
            foreach (var meta in reciever.MetaDataCollection)
            {
                HandleMetadata(context, meta);
            }

            context.AddSource($"bindgen.g.cs",
                SourceText.From(@"global using JSCallBack=System.Action<BlazorBindGen.JObjPtr[]>;
global using BlazorBindGen.Attributes;
global using BlazorBindGen;",
                Encoding.UTF8));

        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new BindingSyntaxReciever());
        }
        private static int nameCount = 0;
        private void HandleMetadata(GeneratorExecutionContext context, Metadata data)
        {
            using StringWriter ss = new();
            using IndentedTextWriter writer = new(ss);
            var usings = data.GetUsings();
            writer.WriteLine(usings);
            writer.WriteLine(@"using System.Threading.Tasks;");

            var @namespace = data.GetNamespace();
            writer.WriteLine(@namespace is null ? "" : $"namespace {@namespace};");
            writer.WriteLine("#nullable enable");

            if (data.AccessModifier() is null || !data.AccessModifier().Value.Any(x => x.ValueText == "partial"))
            {
                ReportDiagonostics("No partial access modifiers found on type ", data, context);
                return;
            }
            if (data.AttribTypes == AttribTypes.JSObject && data.AccessModifier().Value.Any(x => x.ValueText == "static"))
            {
                ReportDiagonostics("Object annotated with JSObject can't be static, but found a vialating Type ", data, context);
                return;
            }

            writer.WriteLine(ClassHeader(data));
            writer.WriteLine("{");
            writer.Indent++;

            GenerateInit(data, writer);

            var members = data.GetMembers();

            foreach (var member in members.GroupBy(x=>x.AttribType))
            {
                //generate properties
                if (member.Key==AttributeTypes.Property)
                    GenerateFieldsProperties(member, data, context, writer);
                //generate functions
                else if (member.Key==AttributeTypes.Function)
                    GenerateFunctions(member, data, context, writer);
                //generate callback
                else if (member.Key == AttributeTypes.Callback)
                    GenerateCallbacks(member, data, context, writer);
                else if (member.Key == AttributeTypes.Construct)
                    //generate constructor for ref type 
                    GenerateFunctions(member, data, context, writer, true);
            }


            writer.Indent--;
            writer.WriteLine("}");
            writer.WriteLine("#nullable restore");

            Console.WriteLine(ss.ToString());

            context.AddSource($"{data.GetName()}_{nameCount++}.g.cs", SourceText.From(ss.ToString(), System.Text.Encoding.UTF8));
        }

    
        private string ClassHeader(Metadata data)
        {
            StringBuilder sb = new("");
            sb.Append(string.Join(" ", data.AccessModifier().Value.Select(x => x.ValueText)));
            sb.Append(" ");
            sb.Append(data.Keyword());
            sb.Append(" ");
            sb.Append(data.GetName());
            sb.Append(data.GetGenericTypes());

            if (data.AttribTypes == AttribTypes.JSObject)
            {
                sb.AppendLine(":BlazorBindGen.IJSObject");
            }
            return sb.ToString();
        }
        private void GenerateInit(Metadata data, IndentedTextWriter writer)
        {
            var isStatic = data.IsStatic();
            if (data.AttribTypes == AttribTypes.Window && isStatic)
            {
                writer.WriteLine("internal static BlazorBindGen.JObjPtr _ptr => BlazorBindGen.BindGen.Window;");
            }
            else if (data.AttribTypes == AttribTypes.Window)
            {
                writer.WriteLine("internal BlazorBindGen.JObjPtr _ptr => BlazorBindGen.BindGen.Window;");
            }
            else
            {
                var name = data.GetName();
                writer.WriteLine("internal BlazorBindGen.JObjPtr _ptr;");
                writer.WriteLine($"internal {name}(BlazorBindGen.JObjPtr ptr)");
                writer.WriteLine("{");
                writer.Indent++;
                writer.WriteLine("_ptr = ptr;");
                writer.Indent--;
                writer.WriteLine('}');

                if (data.Attribute.ArgumentList is not null)
                    if (data.Attribute.ArgumentList.Arguments.Count >= 1)
                    {
                        var value=data.Attribute.ArgumentList.Arguments[0].Expression.ToString();
                        writer.WriteLine($"public static async ValueTask<{name}> ImportAsync()");
                        writer.WriteLine("{");
                        writer.Indent++;
                        writer.WriteLine($"return new {name}(await BlazorBindGen.BindGen.ImportRefAsync({value}));");
                        writer.Indent--;
                        writer.WriteLine("}");

                    }
            }
        }

        private void GenerateFieldsProperties(IEnumerable<MemberMetadata> props, Metadata data, GeneratorExecutionContext context, IndentedTextWriter writer)
        {
            foreach (var m in props)
            {
                var propInfo = GetPropertyInfo(m);
                if (m.Member is not FieldDeclarationSyntax field)
                    continue;

                if (propInfo.Name is null)
                {
                    propInfo.Name = field.Declaration.Variables[0].Identifier.ValueText;
                }
                if(field.Modifiers.Any(x =>x.ValueText == "public" || x.ValueText == "protected" || x.ValueText == "internal"))
                {
                    ReportDiagonostics("Fields can't be public, protected or internal for type", data, context);
                    continue;
                }
                if (field.Declaration.Variables.Count > 1)
                {
                    ReportDiagonostics($"More than one field variables defined in same line in Type ", data, context);
                    continue;
                }
                if (!propInfo.GenerateGetter && !propInfo.GenerateSetter)
                {
                    ReportDiagonostics($"A field `{field.Declaration.Variables[0].Identifier.ValueText}` without getter and setter can't be defined ", data, context);
                    continue;
                }
                if (field.Modifiers.Any(x => x.ValueText == "const") || field.Modifiers.Any(x => x.ValueText == "readonly"))
                {
                    ReportDiagonostics($"A JS interopable field `{field.Declaration.Variables[0].Identifier.ValueText}` cant be const or readonly  ", data, context);
                    continue;
                }
                if (field.Modifiers.Any(x => x.ValueText == "static") && !data.IsStatic())
                {
                    ReportDiagonostics($"A JS interopable field `{field.Declaration.Variables[0].Identifier.ValueText}` cant be static when class itself is not static for type ", data, context);
                    continue;
                }

                writer.Write("public ");
                writer.Write(string.Join(" ", field.Modifiers
                    .Where(x => x.ValueText != "private")
                    .Select(x => x.ValueText)));
                writer.Write(" ");

                var isRefType = IsRefType(field, data, context);

                //type can be ref type also check if base class of any type is IJSObject type
                writer.Write(field.Declaration.Type.ToString());

                writer.Write(" ");

               
                string name = data.Attribute.ArgumentList is not null ? m.Attribute.ArgumentList.Arguments[0].Expression.ToString().Trim('"'):ToggleFirstLetterCase(propInfo.Name);
                writer.WriteLine(name);
                writer.WriteLine("{");
                writer.Indent++;

                //getter
                if (propInfo.GenerateGetter)
                {
                    writer.WriteLine("get");
                    writer.WriteLine("{");
                    writer.Indent++;

                    if (isRefType)
                    {
                        writer.Write("return new ");
                        writer.Write(field.Declaration.Type.ToString());
                        writer.WriteLine($"(_ptr.PropRef(\"{propInfo.Name}\"));");
                    }
                    else if (field.Declaration.Type.ToString().EndsWith("JObjPtr"))
                    {
                        writer.Write("return ");
                        writer.WriteLine($"_ptr.PropRef(\"{propInfo.Name}\");");
                    }
                    else
                    {
                        writer.Write($"return _ptr.PropVal<{field.Declaration.Type}>(\"");
                        writer.Write(propInfo.Name);
                        writer.WriteLine("\");");
                    }


                    writer.Indent--;
                    writer.WriteLine("}");
                }

                //setter
                if (propInfo.GenerateSetter)
                {
                    writer.WriteLine("set");
                    writer.WriteLine("{");
                    writer.Indent++;

                    if (isRefType)
                    {
                        writer.WriteLine($"_ptr.SetPropRef(\"{propInfo.Name}\",value._ptr);");
                    }
                    else if (field.Declaration.Type.ToString().EndsWith("JObjPtr"))
                    {
                        writer.WriteLine($"_ptr.SetPropRef(\"{propInfo.Name}\",value);");
                    }
                    else
                    {
                        writer.WriteLine($"_ptr.SetPropVal(\"{propInfo.Name}\",value);");
                    }


                    writer.Indent--;
                    writer.WriteLine("}");
                }

                writer.Indent--;
                writer.WriteLine("}");
            }
        }
        private void GenerateFunctions(IEnumerable<MemberMetadata> props, Metadata data, GeneratorExecutionContext context, IndentedTextWriter writer,bool isConstruct=false)
        {
            foreach (var f in props)
            {
                if (f.Member is not MethodDeclarationSyntax method)
                    continue;

                if (method.Modifiers.Any(x => x.ValueText == "static") && !data.IsStatic())
                {
                    ReportDiagonostics($"A JS interopable function `{method.Identifier.ValueText}` cant be static when class itself is not static for type ", data, context);
                    continue;
                }
                if (!method.Modifiers.Any(x => x.ValueText == "partial"))
                {
                    ReportDiagonostics($"A JS interopable function `{method.Identifier.ValueText}` should have partial modifier for type ", data, context);
                    continue;
                }
                if (isConstruct && !IsReturnTypeRef(method, context, data))
                {
                    ReportDiagonostics("Return type must have attribute `JSObject` for Construct attribute in type ", data, context);
                    continue;
                }


                var methodInfo = GetMethodInfo(f, context, data,isConstruct);

                writer.Write(string.Join(" ", method.Modifiers.Select(x => x.ValueText)));
                //write return type
                writer.Write(" ");
                writer.Write(method.ReturnType.ToString());
                writer.Write(" ");
                //write name
                writer.Write(method.Identifier.ValueText);
                writer.Write("(");
                if (method.ParameterList is not null)
                {
                    writer.Write(method.ParameterList.Parameters.ToString());
                }
                writer.WriteLine(")");
                writer.WriteLine("{");
                writer.Indent++;
                //writeBody
                {

                    if (!methodInfo.IsVoid || methodInfo.IsValueTaskOnly)
                    {
                        writer.Write("return ");
                    }

                    var isRef = IsReturnTypeRef(method, context, data);
                    var funcName = "Call";

                    if (isRef || methodInfo.ReturnFullName == "BlazorBindGen.JObjPtr")
                        funcName += "Ref";

                    if (methodInfo.IsVoid)
                        funcName += "Void";
                    if (methodInfo.IsAsync && methodInfo.RequireAwait)
                        funcName += "AwaitedAsync";
                    else if (methodInfo.IsAsync)
                        funcName += "Async";

                    if (!methodInfo.IsVoid && !isRef && methodInfo.ReturnFullName != "BlazorBindGen.JObjPtr")
                        if (!methodInfo.IsValueTaskOnly && methodInfo.ReturnFullName.StartsWith("System.Threading.Tasks.ValueTask<"))
                        {
                            int ind = methodInfo.ReturnFullName.IndexOf("ValueTask<");
                            var nt=methodInfo.ReturnFullName.Substring(ind + 10);
                            nt=nt.Remove(nt.Length - 1, 1);
                            funcName += $"<{nt}>";
                        }
                        else
                            funcName += $"<{method.ReturnType}>";

                    if (isConstruct)
                        funcName = "Construct";

                    var finalStatement = $"_ptr.{funcName}(";

                    //parse parameters
                    var semanticModel = context.Compilation
                           .GetSemanticModel(data.DataType.SyntaxTree);

                    IMethodSymbol symbol = (IMethodSymbol)semanticModel
                        .GetDeclaredSymbol(method);

                    finalStatement += $"\"{methodInfo.Name}\"";
                    if (method.ParameterList is not null)
                    {
                        int c = symbol.Parameters.Length;
                        if(c>=1)
                            finalStatement += ",";

                        int i = 0;
                        foreach (var p in symbol.Parameters)
                        {
                            bool isRefParam = p.Type
                                .GetAttributes()
                                .Any(x => x.AttributeClass
                                .ToString() == "BlazorBindGen.Attributes.JSObjectAttribute");
                            var attrib = p.Type.GetAttributes().FirstOrDefault();
                            bool isCallbackType = p.Type.GetAttributes()
                                .Any(x => x.AttributeClass
                                .ToString().Equals("BlazorBindGen.Attributes.JSCallbackAttribute"));

                            if (isCallbackType)
                            {
                                finalStatement += CreateLambdaFromHandler(p, method.ParameterList.Parameters[i].Identifier.ValueText,semanticModel);
                            }
                            else
                            {
                                finalStatement += method.ParameterList.Parameters[i].Identifier.ValueText;
                                if (isRefParam)
                                {
                                    finalStatement += "._ptr";
                                }
                            }
                            if (i != c - 1)
                                finalStatement += ',';
                            i++;
                        }
                    }

                    finalStatement += ")";


                    if (isRef)
                        finalStatement = $"new {method.ReturnType}({finalStatement})";

                    writer.Write(finalStatement);
                    writer.WriteLine(";");
                }
                writer.Indent--;
                writer.WriteLine("}");

            }
        }
        private void GenerateConstruct(IEnumerable<MemberMetadata> enumerable, Metadata data, GeneratorExecutionContext context, IndentedTextWriter writer)
        {
            foreach (var f in enumerable)
            {
                if (f.Member is not MethodDeclarationSyntax method)
                    continue;

                if (method.Modifiers.Any(x => x.ValueText == "static") && !data.IsStatic())
                {
                    ReportDiagonostics($"A JS interopable function `{method.Identifier.ValueText}` cant be static when class itself is not static for type ", data, context);
                    continue;
                }
                if (!method.Modifiers.Any(x => x.ValueText == "partial"))
                {
                    ReportDiagonostics($"A JS interopable function `{method.Identifier.ValueText}` should have partial modifier for type ", data, context);
                    continue;
                }
                var isRef = IsReturnTypeRef(method, context, data);

                if( !isRef )
                {
                    ReportDiagonostics("Return type must have attribute `JSObject` for Construct attribute in type ",data,context);
                    continue;
                }

                var methodInfo = GetMethodInfo(f, context, data);

                writer.Write(string.Join(" ", method.Modifiers.Select(x => x.ValueText)));
                //write return type
                writer.Write(" ");
                writer.Write(method.ReturnType.ToString());
            }
        }
        private string CreateLambdaFromHandler(IParameterSymbol param,string varName, SemanticModel semModel)
        {
            StringBuilder sb = new StringBuilder("(JObjPtr[] result)=>");
            var pnode = param.Type.DeclaringSyntaxReferences.FirstOrDefault().GetSyntax();
            if(pnode is DelegateDeclarationSyntax del)
            {
                sb.Append("{");

                sb.Append($"if({varName} is null)");
                sb.Append("return; ");

                ///map params
                string leftParam = "";
                if (del.ParameterList is not null)
                {
                    int i = 0;
                    var namesList = new List<string>();
                    foreach (var p in del.ParameterList.Parameters)
                    {
                        sb.Append($"var p{i} = ");
                        IParameterSymbol symbol = (IParameterSymbol)semModel.GetDeclaredSymbol(p);
                        var attr = symbol.Type.GetAttributes();
                        var isRef = symbol.Type.GetAttributes()
                            .Any(x => x.AttributeClass
                            .ToString() == "BlazorBindGen.Attributes.JSObjectAttribute")
                            || symbol.Type.AllInterfaces.Any(x => x.ToString() == "BlazorBindGen.IJSObject");

                        var isPtr = symbol.Type.ToString() == "BlazorBindGen.JObjPtr";

                        if (isPtr)
                            sb.Append($"result[{i}]; ");
                        else if (isRef)
                            sb.Append($"new {p.Type}(result[{i}]); ");
                        else
                            sb.Append($"result[{i}].To<{p.Type}>(); ");
                        namesList.Add("p" + i);

                        i++;
                    }
                    leftParam = string.Join(",", namesList);

                }
                sb.Append($"{varName}?.Invoke({leftParam}); ");

                sb.Append("}");
            }
            return sb.ToString();
        }

        private void GenerateCallbacks(IEnumerable<MemberMetadata> enumerable, Metadata data,
            GeneratorExecutionContext context, IndentedTextWriter writer)
        {
            var semModel =context.Compilation.GetSemanticModel(data.DataType.SyntaxTree);

            foreach (var f in enumerable)
            {
                if (f.Member is not DelegateDeclarationSyntax del)
                    continue;
                var name = del.Identifier.ValueText;
                if(!name.EndsWith("Handler"))
                {
                    ReportDiagonostics($"delegate name should be {name}Handler for type ",data,context);
                    continue;
                }
                if (!del.ReturnType.ToString().Equals("void"))
                {
                    ReportDiagonostics($"delegate with non void return type not supported for `{name}` in type ", data, context);
                    continue;
                }
                var eventName = name.Replace("Handler",string.Empty);

                writer.WriteLine($"public event {name}? {eventName};");
                var callbackStubName = $"{name}CallbackStubFunc";
                //create a callback bodymapper
                writer.WriteLine($"private void {callbackStubName}(JObjPtr[] result)");
                writer.WriteLine("{");
                writer.Indent++;

                writer.WriteLine($"if({eventName} is null)");
                writer.Indent++;
                writer.WriteLine("return;");
                writer.Indent--;


                ///map params
                string leftParam = "";
                if(del.ParameterList is not null)
                {
                    int i = 0;
                    var namesList = new List<string>();
                    foreach(var p in del.ParameterList.Parameters)
                    {
                        writer.Write($"var p{i} = ");
                        IParameterSymbol symbol = (IParameterSymbol)semModel.GetDeclaredSymbol(p);
                        var attr = symbol.Type.GetAttributes();
                        var isRef = symbol.Type.GetAttributes()
                            .Any(x => x.AttributeClass
                            .ToString() == "BlazorBindGen.Attributes.JSObjectAttribute")
                            || symbol.Type.AllInterfaces.Any(x => x.ToString() == "BlazorBindGen.IJSObject");

                        var isPtr = symbol.Type.ToString()=="BlazorBindGen.JObjPtr";

                        if (isPtr)
                            writer.WriteLine($"result[{i}];");
                        else if (isRef)
                            writer.WriteLine($"new {p.Type}(result[{i}]);");
                        else
                            writer.WriteLine($"result[{i}].To<{p.Type}>();");
                        namesList.Add("p" + i);

                        i++;
                    }
                    leftParam=string.Join(",", namesList);
                    
                }
                writer.WriteLine($"{eventName}?.Invoke({leftParam});");

                writer.Indent--;
                writer.WriteLine("}");
                //create IJScallback 
                /*
                  private void OnPredictCallback(JObjPtr[] result)
                    if(OnPredict==null) return;
                 */
            }
        }

        private bool IsRefType(FieldDeclarationSyntax field, Metadata data, GeneratorExecutionContext context)
        {
            var semanticModel = context.Compilation
               .GetSemanticModel(data.DataType.SyntaxTree);

            IFieldSymbol symbol = (IFieldSymbol)semanticModel
                .GetDeclaredSymbol(field.Declaration.Variables[0]);

            var interfaces = symbol.Type.AllInterfaces;
            var attributes = symbol.Type.GetAttributes();
            return interfaces.Any(x => x.ToString() == "BlazorBindGen.IJSObject")
                || attributes.Any(x => x.AttributeClass.ToString() == "BlazorBindGen.Attributes.JSObjectAttribute");
        }
        private string ToggleFirstLetterCase(string str)
        {
            if (char.IsUpper(str[0]))
            {
                return char.ToLower(str[0]) + str.Substring(1);
            }
            else
            {
                return char.ToUpper(str[0]) + str.Substring(1);
            }
        }
        private PropertyInfo GetPropertyInfo(MemberMetadata member)
        {

            if (member.Member is not FieldDeclarationSyntax f)
                return new PropertyInfo(true, true, null);

            var attr = member.Attribute.ArgumentList;

            if (attr is null || attr.Arguments.Count == 0)
                return new PropertyInfo(true, true, null);

            if (attr.Arguments.Count >= 1 && attr.Arguments[0].Expression is LiteralExpressionSyntax syn)
            {
                if (syn.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralExpression))
                {
                    var name = syn.Token.ValueText;
                    var getter = attr.Arguments.Count >= 2 ? attr.Arguments[1]
                            .Expression.ToString().Contains("true") : true;
                    var setter = attr.Arguments.Count >= 3 ? attr.Arguments[2]
                            .Expression.ToString().Contains("true") : true;
                    return new PropertyInfo(getter, setter, name);
                }
                else
                {
                    var getter = attr.Arguments.Count >= 1 ? attr.Arguments[0]
                            .Expression.ToString().Contains("true") : true;
                    var setter = attr.Arguments.Count >= 2 ? attr.Arguments[1]
                            .Expression.ToString().Contains("true") : true;

                    return new PropertyInfo(getter, setter, null);
                }
            }

            return new PropertyInfo(true, true, null);
        }
        private MethodInfo GetMethodInfo(MemberMetadata member, GeneratorExecutionContext context, Metadata data,bool isConstruct=false)
        {

            if (member.Member is not MethodDeclarationSyntax f)
                return null;

            var attr = member.Attribute.ArgumentList;
            string name = null;
            if (attr is not null && attr.Arguments.Count >= 1 && attr.Arguments[0].Expression is LiteralExpressionSyntax syn)
            {
                if (syn.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralExpression))
                {
                    name = syn.Token.ValueText;
                }

            }
            name ??= f.Identifier.ValueText;
            var returnType = GetFullReturnTypeName(f, context, data);
            var parameterList = f.ParameterList;
            bool isVoid = returnType.ToString() == "void";
            bool isAsync = returnType.StartsWith("System.Threading.Tasks.ValueTask");

            bool isvt = returnType.Equals("System.Threading.Tasks.ValueTask");


            if (returnType.StartsWith("System.Threading.Tasks.Task"))
            {
                ReportDiagonostics($"Use ValueTask instead of Task in Type : ", data, context);
            }

            bool requireAwait = f.Modifiers.Any(x => x.ValueText == "async");
            return new MethodInfo(name, returnType, isAsync, isVoid, requireAwait, isvt);
        }
        private string GetFullReturnTypeName(MethodDeclarationSyntax type, GeneratorExecutionContext context, Metadata data)
        {
            var semanticModel = context.Compilation
              .GetSemanticModel(data.DataType.SyntaxTree);

            IMethodSymbol symbol = (IMethodSymbol)semanticModel
                .GetDeclaredSymbol(type);

            return symbol.ReturnType.ToString();
        }
        private bool IsReturnTypeRef(MethodDeclarationSyntax type, GeneratorExecutionContext context, Metadata data)
        {
            var semanticModel = context.Compilation
              .GetSemanticModel(data.DataType.SyntaxTree);

            IMethodSymbol symbol = (IMethodSymbol)semanticModel
                .GetDeclaredSymbol(type);

            return symbol.ReturnType.GetAttributes().Any(x => x.AttributeClass.ToString() == "BlazorBindGen.Attributes.JSObjectAttribute");
        }

        private void ReportDiagonostics(string Msg, Metadata data, GeneratorExecutionContext context)
        {
            ISymbol symbol = context.Compilation
                .GetSemanticModel(data.DataType.SyntaxTree).GetDeclaredSymbol(data.DataType);

            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "SG0001",
                    "partial attribute missing",
                    $"{Msg} {symbol.Name}",
                    "Error",
                    DiagnosticSeverity.Error,
                    true), symbol.Locations.FirstOrDefault(),
                symbol.Name, symbol.Name));
        }
    }
}
