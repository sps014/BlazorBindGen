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
            bool isWindow = attribute.Name.ToString().EndsWith(GetAttributeShortName<JSWindowAttribute>());
            var type = isWindow ? AttribTypes.Window : AttribTypes.JSObject;
            if ( (isWindow
                || attribute.Name.ToString().EndsWith(GetAttributeShortName<JSObjectAttribute>()))
                && attribute.Parent is AttributeListSyntax attributes)
            {
                if (attributes.Parent is RecordDeclarationSyntax rec)
                {
                    MetaDataCollection.Add(new Metadata(rec, SupportedTypes.Record, attribute,type));
                }
                else if(attributes.Parent is StructDeclarationSyntax str)
                {
                    MetaDataCollection.Add(new Metadata(str, SupportedTypes.Struct, attribute, type));
                }
                else if (attributes.Parent is ClassDeclarationSyntax @class)
                {
                    MetaDataCollection.Add(new Metadata(@class, SupportedTypes.Class, attribute, type));
                }
            }
        }
    }
    internal static string GetAttributeShortName<T>() where T : Attribute =>
         typeof(T).Name.Replace("Attribute", string.Empty);
}
