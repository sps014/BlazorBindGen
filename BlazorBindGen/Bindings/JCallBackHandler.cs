using Microsoft.JSInterop;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace BlazorBindGen.Bindings;
/// <summary>
/// A class to handle JS Callback 
/// </summary>
internal class JCallBackHandler
{
    /// <summary>
    /// Dictionary to get synchronized callbacks result from JS or Error
    /// </summary>
    internal static readonly ConcurrentDictionary<long, (object? Value, string? Error)> ErrorMessages = new();
    
    /// <summary>
    /// Keeps track of the last sync id for async calls in JS so both Can be synchronized
    /// </summary>
    internal static long SyncCounter = 0;

    /// <summary>
    /// Internally gets called from JS where Value for JS serializable types and error is proppagated 
    /// </summary>
    [Obsolete("Not Intended for use outside of BlazorBindGen")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [JSInvokable("errorMessage")]
    public void ErrorMessageCallback(long callBackId, string? error,object? value)
    {
        ErrorMessages.TryAdd(callBackId, (value, error));
        OnValueOrErrorCallback?.Invoke(callBackId);
    }

    public delegate void OnValueOrErrorCallbackHandler(long callBackId);
    public static event OnValueOrErrorCallbackHandler? OnValueOrErrorCallback;
}