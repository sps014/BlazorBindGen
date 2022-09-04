using System.Collections.Generic;
using System.Linq;
using BlazorBindGen.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static BlazorBindGenerator.BindingSyntaxReciever;

namespace BlazorBindGenerator;

internal class Metadata
{
    public SyntaxNode DataType { get; set; }
    public SupportedTypes Type { get; set; }
    public AttributeSyntax Attribute { get;set; }
    public AttribTypes AttribTypes { get; set; }
    public Metadata(SyntaxNode dataType, SupportedTypes type, AttributeSyntax attribute, AttribTypes attribTypes)
    {
        DataType = dataType;
        Type = type;
        Attribute = attribute;
        AttribTypes = attribTypes;
    }

    public string GetName()
    {
        if (DataType is RecordDeclarationSyntax rec)
            return rec.Identifier.ToString();

        else if (DataType is StructDeclarationSyntax str)
            return str.Identifier.ToString();

        else if (DataType is ClassDeclarationSyntax @class)
            return @class.Identifier.ToString();

        return string.Empty;
    }
    public bool IsStatic()
    {
        var mod = AccessModifier();
        if (mod is null)
            return false;
        return mod.Value.Any(x=>x.ValueText=="static");
    }
    public SyntaxTokenList? AccessModifier()
    {
        if (DataType is RecordDeclarationSyntax rec)
            return rec.Modifiers;

        else if (DataType is StructDeclarationSyntax str)
            return str.Modifiers;

        else if (DataType is ClassDeclarationSyntax @class)
            return @class.Modifiers;

        return null;

    }
    public string Keyword()
    {
        if (DataType is RecordDeclarationSyntax rec)
            return rec.Keyword.ValueText;

        else if (DataType is StructDeclarationSyntax str)
            return str.Keyword.ValueText;

        else if (DataType is ClassDeclarationSyntax @class)
            return @class.Keyword.ValueText;

        return string.Empty;

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
                    //handle both delegate and field cases
                    if (attr.Name.ToString().EndsWith(GetAttributeShortName<JSPropertyAttribute>()) && member.Parent is not DelegateDeclarationSyntax)
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
                    else if (attr.Name.ToString().EndsWith(GetAttributeShortName<JSCallbackAttribute>()))
                    {
                        filteredMembers.Add(new MemberMetadata(MemberType.Delegate, member, attr, AttributeTypes.Callback));
                    }
                }
            }
            
        }
        
        return filteredMembers;
    }

    public string GetNamespace()
    {
        return GetNamespaceInternal(DataType);
    }
    public string GetGenericTypes()
    {
        string res = "<";
        TypeParameterListSyntax? typeParam=null;
        if (DataType is RecordDeclarationSyntax rec)
            typeParam = rec.TypeParameterList;
        else if (DataType is StructDeclarationSyntax str)
            typeParam = str.TypeParameterList;
        else if (DataType is ClassDeclarationSyntax @class)
            typeParam = @class.TypeParameterList;

        if (typeParam is null)
            return string.Empty;

        res += string.Join(",",typeParam.Parameters.Select(x=>x.Identifier.ValueText));

        
        return string.Empty;
    }
    private string GetNamespaceInternal<T>(T node) where T : SyntaxNode
    {
        var parent = node.Parent;
        while (parent.IsKind(SyntaxKind.ClassDeclaration) || parent.IsKind(SyntaxKind.StructDeclaration)
            || parent.IsKind(SyntaxKind.RecordDeclaration) || parent.IsKind(SyntaxKind.RecordStructDeclaration))
        {
            parent = parent.Parent;
        }
        if (parent is NamespaceDeclarationSyntax ns)
            return ns.Name.ToString();
        if (parent is FileScopedNamespaceDeclarationSyntax fs)
            return fs.Name.ToString();
        return null;
    }
    public string GetUsings()
    {
        var usings = DataType.SyntaxTree.GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>();
        return string.Join("", usings.Select(u => u.ToFullString()));
    }

}
internal enum SupportedTypes
{
    Struct,
    Class,
    Record
}
internal enum AttribTypes
{
    Window,
    JSObject
}