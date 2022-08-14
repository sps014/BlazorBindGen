using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlazorBindGenerator;

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
internal class PropertyInfo
{
    public bool GenerateGetter { get; }
    public bool GenerateSetter { get; }
    public string Name { get; set; }
    internal PropertyInfo(bool getter, bool setter, string name)
    {
        GenerateGetter = getter;
        GenerateSetter = setter;
        Name = name;
    }
}
internal enum MemberType
{
    Field,
    Function
}
internal enum AttributeTypes
{
    Property,
    Construct,
    Function,
    Event,
    ConstructFor
}
