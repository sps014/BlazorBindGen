using System;
using Microsoft.CodeAnalysis;

namespace BlazorBindGenerator
{
    [Generator]
    public class BindGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not BindingSyntaxReciever reciever)
                return;
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new BindingSyntaxReciever());
        }
    }
}
