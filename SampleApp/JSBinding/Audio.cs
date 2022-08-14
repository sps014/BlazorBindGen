using BlazorBindGen.Attributes;

namespace SampleApp.JSBinding;

[JSWindow]
public partial class Audio
{
    [JSProperty]
    public int A;
    [JSFunction]
    public partial void B();
}
