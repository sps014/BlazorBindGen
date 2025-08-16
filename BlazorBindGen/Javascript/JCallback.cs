using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.JSInterop;

namespace BlazorBindGen.Javascript;

/// <summary>
/// This class receives javascript events from the browser and forwards them to the appropriate event handlers.
/// </summary>
internal class JCallback
{
    /// <summary>
    /// Dotnet Object that is used to send the callback from JS side to the .NET side.
    /// </summary>
    internal readonly DotNetObjectReference<JCallback> DotNet;
    
    /// <summary>
    /// Function that will be executed when the callback is received
    /// </summary>
    private Action<JObjPtr[]> Executor { get; }
    
    /// <summary>
    /// register a callback from JS side
    /// </summary>
    /// <param name="action">function that will be executed</param>
    public JCallback([NotNull] Action<JObjPtr[]> action)
    {
        DotNet = DotNetObjectReference.Create(this);
        Executor = action;
    }
    
    /// <summary>
    /// Called from JS side when the JS side callback is received
    /// </summary>
    [Obsolete("Meant for Internal Use Only")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [JSInvokable("ExecuteInCSharp")]
    public async Task CallMe(int hash, int argLength)
    {
        //Get array of arguments as JObjPtr
        var ptr = await GetArgAsPtrAsync(hash);
        //Create array of JObjPtr with the length of the arguments
        var arr = new JObjPtr[argLength];
        
        //Get each argument from Arg Array
        for (var i = 0; i < argLength; i++)
            if(BindGen.IsWasm)
                arr[i] = ptr.PropRef($"{i}");
            else
                arr[i] = await ptr.PropRefAsync($"{i}");

        Executor.Invoke(arr);
    }
    /// <summary>
    /// Cleanup all arguments for assignment to JObjPtr
    /// </summary>
    /// <param name="hash"> Hash of callback message </param>
    /// <returns></returns>
    private async ValueTask<JObjPtr> GetArgAsPtrAsync(int hash)
    {
        JObjPtr ptrs = new();
        if (BindGen.IsWasm)
            BindGen.WasmModule.InvokeVoid("CleanUpArgs", hash, ptrs.Hash);
        else
            await BindGen.ServerModule.InvokeVoidAsync("CleanUpArgs", hash, ptrs.Hash);
        return ptrs;
    }
}