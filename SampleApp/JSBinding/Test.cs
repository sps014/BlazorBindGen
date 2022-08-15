using BlazorBindGen.Attributes;
using BlazorBindGen;
using System.Threading.Tasks;

namespace SampleApp.JSBinding;

[JSWindow]
public static partial class Win
{
    [JSFunction("alert")]
    public static partial void Alert(object data);
    
    [JSProperty]
    private static string title;
}

