using BlazorBindGen.Attributes;
using BlazorBindGen;
namespace SampleApp.JSBinding;

[JSWindow]
public partial class Audio
{
    [JSProperty(true)]
    public C A;

    [JSProperty]
    public int C;

    [JSProperty]
    public JObjPtr Another;

    [JSFunction]
    public partial void B();
}

[JSObject]
public partial class C: IJSObject
{
    
}