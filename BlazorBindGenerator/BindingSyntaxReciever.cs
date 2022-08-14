using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BlazorBindGen.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlazorBindGenerator;

internal class BindingSyntaxReciever:ISyntaxReceiver
{
    public HashSet<Metadata> MetaDataCollection { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is AttributeSyntax attribute)
        {
            if ((attribute.Name.ToString().EndsWith(GetAttributeShortName<JSWindowAttribute>())
                || attribute.Name.ToString().EndsWith(GetAttributeShortName<JSObjectAttribute>()))
                && attribute.Parent is AttributeListSyntax attributes)
            {
                if (attributes.Parent is RecordDeclarationSyntax rec)
                {
                    MetaDataCollection.Add(new Metadata(rec, SupportedTypes.Record, attribute));
                }
                else if(attributes.Parent is StructDeclarationSyntax str)
                {
                    MetaDataCollection.Add(new Metadata(str, SupportedTypes.Struct, attribute));
                }
                else if (attributes.Parent is ClassDeclarationSyntax @class)
                {
                    MetaDataCollection.Add(new Metadata(@class, SupportedTypes.Class, attribute));
                }
            }
        }
    }
    internal static string GetAttributeShortName<T>() where T : Attribute =>
         typeof(T).Name.Replace("Attribute", string.Empty);
}
