using Microsoft.JSInterop;

namespace BlazorBindGen;

/// <summary>
/// Represents window in javascript.
/// eg. window.document.getElementById("id")  here this is window object Ptr
/// </summary>
public sealed class JWindow : JObjPtr
{
    /// <summary>
    /// Internal Constructor for JWindow so it have only 1 instance
    /// </summary>
    private JWindow() { }
    
    /// <summary>
    /// This function is called once to get window Pointer in JS to be used in C#.
    /// </summary>
    /// <returns>JWindow Pointer</returns>
    internal static async ValueTask<JWindow> CreateJWindowObject()
    {
        //Onetime instance of JWindow is created
        JWindow win = new();
        
        //If Wasm go faster Unmarshalled version of functionCall

        await BindGen.CommonModule.InvokeVoidAsync("CreateWin", win.Hash);
        return win;
    }

}