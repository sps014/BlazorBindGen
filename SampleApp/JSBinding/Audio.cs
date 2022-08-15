using BlazorBindGen.Attributes;
using BlazorBindGen;
using System.Threading.Tasks;

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
    public partial Task B(int a , int c ,C b);
}

[JSObject]
public partial class C
{
    
}