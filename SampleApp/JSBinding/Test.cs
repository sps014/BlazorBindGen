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
    private static Document document;
}
[JSObject]
public partial class Document
{
    [JSProperty]
    private string title;
}
