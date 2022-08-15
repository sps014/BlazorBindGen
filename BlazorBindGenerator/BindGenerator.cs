using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

            foreach (var meta in reciever.MetaDataCollection)
            {
                HandleMetadata(context, meta);
            }

            context.AddSource($"bindgen.g.cs", 
                SourceText.From("global using JSCallBack=System.Action<BlazorBindGen.JObjPtr[]>;",
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
            var @namespace = data.GetNamespace();
            writer.WriteLine(@namespace is null ? "" : $"namespace {@namespace};");

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

            //generate fields
            GenerateFieldsProperties(members.Where(x => x.AttribType == AttributeTypes.Property), data, context, writer);

            //generate functions
            GenerateFunctions(members.Where(x => x.AttribType == AttributeTypes.Function), data, context, writer);
            
            writer.Indent--;
            writer.WriteLine("}");

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
            
            if(data.AttribTypes==AttribTypes.JSObject)
            {
                sb.AppendLine(":BlazorBindGen.IJSObject");
            }
            return sb.ToString();
        }
        private void GenerateInit(Metadata data, IndentedTextWriter writer)
        {
            var isStatic = data.IsStatic();
            if (data.AttribTypes == AttribTypes.Window && data.IsStatic())
            {
                writer.WriteLine("private static BlazorBindGen.JObjPtr _ptr => BlazorBindGen.BindGen.Window;");
            }
            else if (data.AttribTypes == AttribTypes.Window)
            {
                writer.WriteLine("internal BlazorBindGen.JObjPtr _ptr => BlazorBindGen.BindGen.Window;");
            }
            else
            {
                writer.WriteLine("private BlazorBindGen.JObjPtr _ptr;");
                writer.WriteLine($"internal {data.GetName()}(BlazorBindGen.JObjPtr ptr)");
                writer.WriteLine("{");
                writer.Indent++;
                writer.WriteLine("_ptr = ptr;");
                writer.Indent--;
                writer.WriteLine('}');
            }
        }

        private void GenerateFieldsProperties(IEnumerable<MemberMetadata> props, Metadata data, GeneratorExecutionContext context, IndentedTextWriter writer)
        {
            foreach (var m in props)
            {
                var propInfo = GetPropertyInfo(m);
                if (m.Member is not FieldDeclarationSyntax field)
                    continue;

                if(propInfo.Name is null)
                {
                    propInfo.Name = ToggleFirstLetterCase(field.Declaration.Variables[0].Identifier.ValueText);
                }
                
                writer.Write(string.Join(" ",field.Modifiers.Select(x=>x.ValueText)));
                writer.Write(" ");

                var isRefType = IsRefType(field, data, context);

                //type can be ref type also check if base class of any type is IJSObject type
                writer.Write(field.Declaration.Type.ToString());

                writer.Write(" ");

                if(field.Declaration.Variables.Count>1)
                {
                    ReportDiagonostics($"More than one field variables defined in same line in Type ", data, context);
                    continue;
                }
                if (!propInfo.GenerateGetter && !propInfo.GenerateSetter)
                {
                    ReportDiagonostics($"A field `{field.Declaration.Variables[0].Identifier.ValueText}` without getter and setter can't be defined ", data, context);
                    continue;
                }
                if (field.Modifiers.Any(x => x.ValueText == "const") || field.Modifiers.Any(x=>x.ValueText=="readonly"))
                {
                    ReportDiagonostics($"A JS interopable field `{field.Declaration.Variables[0].Identifier.ValueText}` cant be const or readonly  ", data, context);
                    continue;
                }
                if(field.Modifiers.Any(x=>x.ValueText=="static") && !data.IsStatic())
                {
                    ReportDiagonostics($"A JS interopable field `{field.Declaration.Variables[0].Identifier.ValueText}` cant be static when class itself is not static for type ", data, context);
                    continue;
                }
                
                writer.WriteLine(propInfo.Name);
                writer.WriteLine("{");
                writer.Indent++;

                //getter
                if(propInfo.GenerateGetter)
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
        private void GenerateFunctions(IEnumerable<MemberMetadata> props, Metadata data, GeneratorExecutionContext context, IndentedTextWriter writer)
        {
            foreach(var f in props)
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


                var methodInfo = GetMethodInfo(f,context,data);

                writer.Write(string.Join(" ", method.Modifiers.Select(x => x.ValueText)));
                //write return type
                writer.Write(" ");
                writer.Write(method.ReturnType.ToString());
                writer.Write(" ");
                //write name
                writer.Write(method.Identifier.ValueText);
                writer.Write("(");
                if(method.ParameterList is not null)
                {
                    writer.Write(method.ParameterList.Parameters.ToString());
                }
                writer.WriteLine(")");
                writer.WriteLine("{");
                writer.WriteLine("}");

            }
        }

        private bool IsRefType(FieldDeclarationSyntax field,Metadata data,GeneratorExecutionContext context)
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
        private MethodInfo GetMethodInfo(MemberMetadata member,GeneratorExecutionContext context,Metadata data)
        {
            
            if (member.Member is not MethodDeclarationSyntax f)
                return null;

            var attr = member.Attribute.ArgumentList;
            string name=null;
            if (attr is not null && attr.Arguments.Count >= 1 && attr.Arguments[0].Expression is LiteralExpressionSyntax syn)
            {
                if (syn.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralExpression))
                {
                    name = syn.Token.ValueText;
                }
                
            }
            name ??= f.Identifier.ValueText;
            var returnType = GetFullReturnTypeName(f,context,data);
            var parameterList = f.ParameterList;
            bool isVoid = returnType.ToString() == "void";
            bool isAsync = returnType.StartsWith("System.Threading.Tasks.ValueTask");
            
            if(returnType.StartsWith("System.Threading.Tasks.Task"))
            {
                ReportDiagonostics($"Use ValueTask instead of Task in Type : ",data,context);
            }
            
            bool requireAwait = f.Modifiers.Any(x => x.ValueText == "async");
            return new MethodInfo(name, returnType, isAsync, isVoid,requireAwait);
        }
        private string GetFullReturnTypeName(MethodDeclarationSyntax type,GeneratorExecutionContext context,Metadata data)
        {
            var semanticModel = context.Compilation
              .GetSemanticModel(data.DataType.SyntaxTree);

            IMethodSymbol symbol =(IMethodSymbol)semanticModel
                .GetDeclaredSymbol(type);
            return symbol.ReturnType.ToString();
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
