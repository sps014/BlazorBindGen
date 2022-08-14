using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BlazorBindGen.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static BlazorBindGenerator.BindingSyntaxReciever;

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

internal class Metadata
{
    private const string Value = "JSConstructFor";

    public SyntaxNode DataType { get; set; }
    public SupportedTypes Type { get; set; }
    public AttributeSyntax Attribute { get;set; }
    public Metadata(SyntaxNode dataType, SupportedTypes type, AttributeSyntax attribute)
    {
        DataType = dataType;
        Type = type;
        Attribute = attribute;
    }
    public IReadOnlyList<MemberMetadata> GetMembers()
    {
        SyntaxList<MemberDeclarationSyntax> members = new();
        if (DataType is RecordDeclarationSyntax rec)
        {
            members= rec.Members;
        }
        
        else if (DataType is StructDeclarationSyntax str)
        {
            members= str.Members;
        }
        
        else if (DataType is ClassDeclarationSyntax @class)
        {
            members= @class.Members;
        }

        var filteredMembers= new List<MemberMetadata>(); 
        foreach(var member in members)
        {
            var attrbs = member.AttributeLists;
            foreach (var attrList in attrbs)
            {
                foreach (var attr in attrList.Attributes)
                {
                    if (attr.Name.ToString().EndsWith(GetAttributeShortName<JSPropertyAttribute>()))
                    {
                        filteredMembers.Add(new MemberMetadata(MemberType.Field, member, attr,AttributeTypes.Property));
                    }
                    else if (attr.Name.ToString().EndsWith(GetAttributeShortName<JSFunctionAttribute>()))
                    {
                        filteredMembers.Add(new MemberMetadata(MemberType.Function, member, attr, AttributeTypes.Function));
                    }
                    else if (attr.Name.ToString().EndsWith(GetAttributeShortName<JSConstructAttribute>()))
                    {
                        filteredMembers.Add(new MemberMetadata(MemberType.Function, member, attr, AttributeTypes.Construct));
                    }
                    else if (attr.Name.ToString().Contains(Value))
                    {
                        filteredMembers.Add(new MemberMetadata(MemberType.Function, member, attr, AttributeTypes.ConstructFor));
                    }
                }
            }
            
        }
        
        return filteredMembers;
    }
     
}

internal class MemberMetadata
{
    public MemberType Type { get; set; }
    public MemberDeclarationSyntax Member { get; set; }
    public AttributeSyntax Attribute { get; set; }
    public AttributeTypes AttribType { get; set; }
    public MemberMetadata(MemberType type, MemberDeclarationSyntax member,
        AttributeSyntax attribute, AttributeTypes types)
    {
        Type = type;
        Member = member;
        Attribute = attribute;
        AttribType = types;
    }
}
internal enum MemberType
{
    Field,
    Function
}
internal enum SupportedTypes
{
    Struct,
    Class,
    Record
}
internal enum AttributeTypes
{
    Property,
    Construct,
    Function,
    Event,
    ConstructFor
}
