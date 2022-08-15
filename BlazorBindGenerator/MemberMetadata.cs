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
internal class MethodInfo
{
    public string Name { get; set; }
    public string ReturnFullName { get; set; }
    public bool IsAsync { get; set; }
    public bool IsVoid { get; set; }
    public bool RequireAwait { get; }
    public MethodInfo(string name, string returnFullName, bool isAsync, bool isVoid,bool requireAwait)
    {
        Name = name;
        ReturnFullName = returnFullName;
        IsAsync = isAsync;
        IsVoid = isVoid;
        RequireAwait =requireAwait;
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
