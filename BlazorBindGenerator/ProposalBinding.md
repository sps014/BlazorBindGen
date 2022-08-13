```cs
[JSWindow(Name="console")]
partial static class Console
{
    [JSFunction(Name = "log")]
    public partial static void Log(string message);
}

[JSWindow(Name="window")]
partial class Window
{
    [JSProperty(Name = "name")]
    private string Name;

}

[JSWindow(Name="document")]
partial class Document
{
    [JSFunction(Name = "getElementById")]
    public partial HtmlElement GetElementById(string id);

    [JSConstructFor(typeof(HtmlElement))]
    public partial HtmlElement HtmlElement(string tagName);
}

[JSObject]
class HtmlElement
{
    [JSProperty(Name = "id")]
    public string Id { get; set; }

    [JSFunction(Name = "childNodes")]
    public HtmlElement[] ChildNodes();

    [JSConstructor]
    private void Init(JObjPtr base);
}

[JSWindow]
public class Audio
{
    [JSConstructor]
    private void Init(string url);
}
[JSObject("remote URL")]
public class P5
{
    [JSConstructor]
    private void Init(int draw);
}
///Generated Code

public partial class Console
{
    [JSFunction(Name = "log")]
    public partial void Log(string message)
    {
        Window["console"].CallVoid("log", message);
    }
}

class HtmlElement
{
    public string id
    [
        get=> _ptr.GetProp<string>("id");
        set=> _ptr.SetProp("id", value);
    ]
    internal HtmlElement(JSObject ptr)
    {
        _ptr = ptr;
    }

    private JobjPtr _ptr;

    internal HtmlElement[] ChildNodes()
    {
        var return _ptr.Call<JobjPtr[]>("childNodes");
    }
}

partial class Document
{
    public partial HtmlElement GetElementById(string id)
    {
        _ptr=_ptr.Call<JobjPtr>("getElementById", id);
    }

    public partial HtmlElement HtmlElement(string tagName)
    {
        return =new HtmlElement(_ptr.Call<JobjPtr>("createElement", tagName));
    }
}
class Audio
{
    private JObjPtr _ptr;
    public Audio(string url)
    {
       _ptr=Window.Construct("Audio", url);
    }
}

var audio = new Audio();

new document.Audio();

```