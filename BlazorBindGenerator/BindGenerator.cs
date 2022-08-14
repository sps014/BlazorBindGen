using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
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

            foreach(var meta in reciever.MetaDataCollection)
            {
                HandleMetadata(context, meta);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new BindingSyntaxReciever());
        }
        private static int nameCount=0;
        private void HandleMetadata(GeneratorExecutionContext context,Metadata data)
        {
            using StringWriter ss = new();
            using IndentedTextWriter writer = new(ss);
            var usings = data.GetUsings();
            writer.WriteLine(usings);
            var @namespace = data.GetNamespace();
            writer.WriteLine(@namespace is null?"":$"namespace {@namespace};");
            
            if(data.AccessModifier() is null || !data.AccessModifier().Value.Any(x=>x.ValueText=="partial"))
            {
                ReportDiagonostics("No partial access modifiers found on type ",data,context);
                return;
            }
            if(data.AttribTypes==AttribTypes.JSObject && data.AccessModifier().Value.Any(x => x.ValueText == "static"))
            {
                ReportDiagonostics("Object annotated with JSObject can't be static, but found a vialating Type ", data, context);
                return;
            }

            writer.WriteLine(ClassHeader(data));
            writer.WriteLine("{");
            writer.Indent++;

            GenerateInit(data,writer);

            var members = data.GetMembers();

            //generate fields
            GenerateFieldsProperties(members.Where(x=>x.AttribType==AttributeTypes.Property),data,context,writer);

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
            sb.Append(data.GetGenericTypes()+":BlazorBindGen.IJSObject");
            return sb.ToString();
        }
        private void GenerateInit(Metadata data,IndentedTextWriter writer)
        {
            var isStatic = data.IsStatic();
            if (data.AttribTypes == AttribTypes.Window && data.IsStatic())
            {
                writer.WriteLine("private static BlazorBindGen.JObjPtr _ptr => BlazorBindGen.BindGen.Window;");
            }
            else if (data.AttribTypes == AttribTypes.Window)
            {
                writer.WriteLine("private BlazorBindGen.JObjPtr _ptr => BlazorBindGen.BindGen.Window;");
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
        }

        private void ReportDiagonostics(string Msg,Metadata data, GeneratorExecutionContext context)
        {
            ISymbol symbol = context.Compilation
                .GetSemanticModel(data.DataType.SyntaxTree).GetDeclaredSymbol(data.DataType);

            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "SG0001",
                    "partial attribute missing",
                    $"{ Msg } { symbol.Name }",
                    "Error",
                    DiagnosticSeverity.Error,
                    true), symbol.Locations.FirstOrDefault(),
                symbol.Name,symbol.Name));
        }
    }
}
