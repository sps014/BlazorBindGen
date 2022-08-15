using BlazorBindGen.Attributes;
using BlazorBindGen;
using System.Threading.Tasks;

namespace SampleApp.JSBinding;

[JSWindow]
public partial class Audio
{
    [JSProperty(true)]
    public Comms A;

    [JSProperty]
    public int C;

    [JSProperty]
    public JObjPtr Another;
    [JSFunction]
    public partial ValueTask<Comms> B(int a, int c, Comms b);
    [JSFunction]
    public partial ValueTask B(int a , int c ,Comms b);

}

[JSObject]
public partial class Comms
{
    
}