using System;
using System.Collections.Generic;
using System.Text;
using BlazorBindGen.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlazorBindGenerator
{
    public class JSBindingSyntaxReciever : ISyntaxReceiver
    {
        public List<(ClassDeclarationSyntax Class,AttributeSyntax Attribute)> ClassDeclarations { get; private set; } = new();
        
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if(syntaxNode is not AttributeSyntax attributeSyntax)
                return;

            if (!attributeSyntax.Name.ToString().EndsWith(GetAttributeShortName<JSModuleAttribute>()))
                return;

            if (attributeSyntax.Parent.Parent is not ClassDeclarationSyntax @class)
                return;

            ClassDeclarations.Add((@class,attributeSyntax));
        }

        internal static string GetAttributeShortName<T>() where T : Attribute =>
         typeof(T).Name.Replace("Attribute", string.Empty);
    }
}
