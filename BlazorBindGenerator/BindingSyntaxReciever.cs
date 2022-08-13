using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlazorBindGenerator
{
    internal class BindingSyntaxReciever:ISyntaxReceiver
    {
        public HashSet<object> DataTypeDeclarations { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            throw new NotImplementedException();
        }
    }
}
