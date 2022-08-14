using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
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
                ReportDiagonostics("No partial access modifiers found on type",data,context);
                return;
            }



            context.AddSource($"{data.GetName()}_{nameCount++}.g.cs", SourceText.From(ss.ToString(), System.Text.Encoding.UTF8));
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
