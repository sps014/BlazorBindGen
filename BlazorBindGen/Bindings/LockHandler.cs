using System.Collections.Concurrent;
using System.Text.Json;
namespace BlazorBindGen.Bindings;

/// <summary>
/// Since there is no synchronization between the JS Await and the C# await, we need to use Wait for
/// method call in JS to complete its asynchronous execution. This is workaround need to replaced in future.
/// </summary>
internal static class LockHandler
{
    /// <summary>
    /// Await process->Semaphores map 
    /// </summary>
    private static ConcurrentDictionary<long, SemaphoreSlim> HoldedTasks = new();

    static LockHandler()
    {
        JCallBackHandler.OnValueOrErrorCallback += JCallBackHandler_OnValueOrErrorCallback;
    }

    private static void JCallBackHandler_OnValueOrErrorCallback(long callBackId)
    {
        if (HoldedTasks.ContainsKey(callBackId))
        {
            HoldedTasks.TryRemove(callBackId, out var semaphore);
            semaphore?.Release();
        }
    }

    /// <summary>
    /// Hold for async void method call in JS Side so C# side can catch up to it's synchronization.
    /// </summary>
    /// <param name="errH">Error Hash Value of the Async Method</param>
    /// <exception cref="Exception">Fetches Error message from JS Side</exception>
    public static async ValueTask HoldVoid(long errH)
    {
        //Wait until Synchronization message is received from JS
        var semaphore = new SemaphoreSlim(0, 1);
        HoldedTasks.TryAdd(errH, semaphore);
        await semaphore.WaitAsync();

        //Remove that Value from the Dictionary
        _ = JCallBackHandler.ErrorMessages.TryRemove(errH, out var tpl);

        //Throw the Exception
        if (!string.IsNullOrWhiteSpace(tpl.Error))
            throw new Exception(tpl.Error);
    }
    /// <summary>
    /// Hold for async non void method call in JS Side so C# side can catch up to it's synchronization.
    /// </summary>
    /// <param name="errH">Error Hash Value of the Async Method</param>
    /// <typeparam name="T">Non Reference Result</typeparam>
    /// <returns>JS Serializable type</returns>
    /// <exception cref="Exception"></exception>
    public static async ValueTask<T?> Hold<T>(long errH)
    {
        var semaphore = new SemaphoreSlim(0, 1);
        HoldedTasks.TryAdd(errH, semaphore);
        await semaphore.WaitAsync();

        _ = JCallBackHandler.ErrorMessages.TryRemove(errH, out var tpl);

        //Throw the Exception if has error
        if (!string.IsNullOrWhiteSpace(tpl.Error))
            throw new Exception(tpl.Error);

        // return default value if  null
        if (tpl.Value is null)
            return default;

        //Deserialize the JS Serialized Object to C# Object
        var json = ((JsonElement)tpl.Value).GetRawText();
        return JsonSerializer.Deserialize<T>(json);
    }
}