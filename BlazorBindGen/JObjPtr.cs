using Microsoft.JSInterop;
using System.Buffers;
using System.Runtime.CompilerServices;
using BlazorBindGen.Bindings;
using BlazorBindGen.Javascript;
using BlazorBindGen.Utils;
using static BlazorBindGen.BindGen;

namespace BlazorBindGen;

/// <summary>
/// Represents Pointer to a JS Object on whose properties and methods, constructors can be called and manipulated
/// </summary>
public class JObjPtr : IEquatable<JObjPtr>
{
    /// <summary>
    /// Current Hash of JS Object used for Memory Management 
    /// </summary>
    internal int Hash { get; private set; }

    /// <summary>
    /// Internal JS Object Pointer Address Allocator (Counter)
    /// </summary>
    private static int _hashCount = 0;

    /// <summary>
    /// Pool of parameters to be reused on multiple calls
    /// </summary>
    private static readonly ArrayPool<ParamInfo> ParamPool = ArrayPool<ParamInfo>.Shared;

    /// <summary>
    /// Internal Constructor helps in getting new hash address
    /// </summary>
    internal JObjPtr()
    {
        Hash = Interlocked.Increment(ref _hashCount);
    }
    ~JObjPtr()
    {
        if (IsWasm)
            _ = Module.InvokeUnmarshalled<long, object>("DeletePtr", Hash);
        else
            Task.Run(() =>
            {
                GeneralizedModule.DisposeAsync().ConfigureAwait(false);
            });
    }
    
    /// <summary>
    /// Get Exact C# Serializable Property Value from JS Object
    /// </summary>
    /// <param name="propertyName">Name of property to get</param>
    /// <typeparam name="T">.net type of property </typeparam>
    /// <returns>exact value of property</returns>
    public T PropVal<T>(string propertyName)
    {
        if (IsWasm)
            return Module.Invoke<T>("PropVal", propertyName, Hash);
        else
            throw PlatformUnsupportedException.Throw();
    }
    public async ValueTask<T> PropValAsync<T>(string propname)
    {
        if (IsWasm)
            return await Module.InvokeAsync<T>("propval", propname, Hash);
        else
            return await GeneralizedModule.InvokeAsync<T>("propval", propname, Hash);
    }
    public JObjPtr PropRef(string propname)
    {
        JObjPtr obj = new();
        if (IsWasm)
            _ = Module.InvokeUnmarshalled<string, int, int, object>("propref", propname, obj.Hash, Hash);
        else
            GeneralizedModule.InvokeVoidAsync("proprefgen", propname, obj.Hash, Hash).GetAwaiter().GetResult();
        return obj;
    }
    public async ValueTask<JObjPtr> PropRefAsync(string propname)
    {
        JObjPtr obj = new();
        if (IsWasm)
            await Module.InvokeVoidAsync("propref", propname, obj.Hash, Hash);
        else
            await GeneralizedModule.InvokeVoidAsync("propref", propname, obj.Hash, Hash);
        return obj;
    }
    public void SetPropVal<T>(string propname, T value)
    {
        if (!IsWasm)
            GeneralizedModule.InvokeVoidAsync("propset", propname, value, Hash).GetAwaiter().GetResult();
        else
            Module.InvokeVoid("propset", propname, value, Hash);
    }

    public void SetPropRef(string propname, JObjPtr obj)
    {
        if (IsWasm)
            _ = Module.InvokeUnmarshalled<string, int, int, object>("propsetref", propname, obj.Hash, Hash);
        else
            GeneralizedModule.InvokeVoidAsync("propsetrefgen", propname, obj.Hash, Hash).GetAwaiter().GetResult();
    }
    public async ValueTask SetPropRefAsync(string propname, JObjPtr obj)
    {
        if (IsWasm)
            _ = Module.InvokeUnmarshalled<string, int, int, object>("propsetref", propname, obj.Hash, Hash);
        else
            await GeneralizedModule.InvokeVoidAsync("propsetrefgen", propname, obj.Hash, Hash);
    }

    public bool IsProp(string propname)
    {
        if (IsWasm)
            return Module.InvokeUnmarshalled<string, int, bool>("isprop", propname, Hash);
        else
            return GeneralizedModule.InvokeAsync<bool>("ispropgen", propname, Hash).GetAwaiter().GetResult();
    }
    public async ValueTask<bool> IsPropAsync(string propname)
    {
        if (IsWasm)
            return Module.InvokeUnmarshalled<string, int, bool>("isprop", propname, Hash);
        else
            return await GeneralizedModule.InvokeAsync<bool>("ispropgen", propname, Hash);
    }
    public bool IsFunc(string propname)
    {
        if (IsWasm)
            return Module.InvokeUnmarshalled<string, int, bool>("isfunc", propname, Hash);
        else
            return GeneralizedModule.InvokeAsync<bool>("isfuncgen", propname, Hash).GetAwaiter().GetResult();
    }
    public async ValueTask<bool> IsFuncAsync(string propname)
    {
        if (IsWasm)
            return Module.InvokeUnmarshalled<string, int, bool>("isfunc", propname, Hash);
        else
            return await GeneralizedModule.InvokeAsync<bool>("isfuncgen", propname, Hash);
    }

    public T Call<T>(string funcName, params object[] param)
    {
        var args = GetParamList(param);
        T res;
        if (IsWasm)
            res = Module.Invoke<T>("func", funcName, args.AsSpan()[..param.Length].ToArray(), Hash);
        else
            res = GeneralizedModule.InvokeAsync<T>("func", funcName, args.AsSpan()[..param.Length].ToArray(), Hash).GetAwaiter().GetResult();

        ParamPool.Return(args);
        return res;
    }

    public async ValueTask<T> CallAsync<T>(string funcname, params object[] param)
    {
        var args = GetParamList(param);
        T res;
        if (IsWasm)
            res = await Module.InvokeAsync<T>("func", funcname, args.AsSpan()[..param.Length].ToArray(), Hash);
        else
            res = await GeneralizedModule.InvokeAsync<T>("func", funcname, args.AsSpan()[..param.Length].ToArray(), Hash);

        ParamPool.Return(args);
        return res;
    }

    public JObjPtr CallRef(string funcname, params object[] param)
    {
        var args = GetParamList(param);
        JObjPtr j = new();
        if (IsWasm)
            Module.InvokeVoid("funcref", funcname, args.AsSpan()[..param.Length].ToArray(), j.Hash, Hash);
        else
            GeneralizedModule.InvokeVoidAsync("funcref", funcname, args.AsSpan()[..param.Length].ToArray(), j.Hash, Hash).GetAwaiter().GetResult();
        ParamPool.Return(args);
        return j;
    }

    public async ValueTask<JObjPtr> CallRefAsync(string funcname, params object[] param)
    {
        JObjPtr j = new();
        var args = GetParamList(param);
        if (IsWasm)
            await Module.InvokeVoidAsync("funcref", funcname, args.AsSpan()[..param.Length].ToArray(), j.Hash, Hash);
        else
            await GeneralizedModule.InvokeVoidAsync("funcref", funcname, args.AsSpan()[..param.Length].ToArray(), j.Hash, Hash);
        ParamPool.Return(args);
        return j;
    }
    public void CallVoid(string funcname, params object[] param)
    {
        var args = GetParamList(param);
        if (IsWasm)
            Module.InvokeVoid("funcvoid", funcname, args.AsSpan()[..param.Length].ToArray(), Hash);
        else
            GeneralizedModule.InvokeVoidAsync("funcvoid", funcname, args.AsSpan()[..param.Length].ToArray(), Hash)
                .GetAwaiter().GetResult();
        ParamPool.Return(args);
    }

    public async ValueTask CallVoidAsync(string funcname, params object[] param)
    {
        var args = GetParamList(param);
        if (IsWasm)
            await Module.InvokeVoidAsync("funcvoid", funcname, args.AsSpan()[..param.Length].ToArray(), Hash);
        else
            await GeneralizedModule.InvokeVoidAsync("funcvoid", funcname, args.AsSpan()[..param.Length].ToArray(), Hash);
        ParamPool.Return(args);
    }

    public async ValueTask<JObjPtr> CallRefAwaitedAsync(string funcname, params object[] param)
    {
        JObjPtr obj = new();
        long errH = Interlocked.Increment(ref JCallBackHandler.SyncCounter);
        var args = GetParamList(param);
        if (IsWasm)
            Module.InvokeVoid("funcrefawait", funcname, args.AsSpan()[..param.Length].ToArray(), errH, obj.Hash, Hash);
        else
            await GeneralizedModule.InvokeVoidAsync("funcrefawait", funcname, args.AsSpan()[..param.Length].ToArray(), errH, obj.Hash, Hash);

        ParamPool.Return(args);

        await LockHandler.HoldVoid(errH);
        return obj;

    }

    public async ValueTask CallVoidAwaitedAsync(string funcname, params object[] param)
    {
        long errH = Interlocked.Increment(ref JCallBackHandler.SyncCounter);
        var args = GetParamList(param);
        if (IsWasm)
            Module.InvokeVoid("funcvoidawait", funcname, args.AsSpan()[..param.Length].ToArray(), errH, Hash);
        else
            await GeneralizedModule.InvokeVoidAsync("funcvoidawait", funcname, args.AsSpan()[..param.Length].ToArray(), errH, Hash);

        ParamPool.Return(args);
        await LockHandler.HoldVoid(errH);
    }

    public async ValueTask<T> CallAwaitedAsync<T>(string funcname, params object[] param)
    {
        long errH = Interlocked.Increment(ref JCallBackHandler.SyncCounter);
        var args = GetParamList(param);
        if (IsWasm)
            Module.InvokeVoid("funcawait", funcname, args.AsSpan()[..param.Length].ToArray(), errH, Hash);
        else
            await GeneralizedModule.InvokeVoidAsync("funcawait", funcname, args.AsSpan()[..param.Length].ToArray(), errH, Hash);

        ParamPool.Return(args);

        return await LockHandler.Hold<T>(errH);

    }
    public string AsJsonText()
    {
        if (IsWasm)
            return Module.Invoke<string>("asjson", Hash);
        else
            return GeneralizedModule.InvokeAsync<string>("asjson", Hash).GetAwaiter().GetResult();

    }
    public async ValueTask<string> AsJsonTextAsync()
    {
        if (IsWasm)
            return await Module.InvokeAsync<string>("asjson", Hash);
        else
            return await GeneralizedModule.InvokeAsync<string>("asjson", Hash);
    }
    public T To<T>()
    {
        if (IsWasm)
            return Module.Invoke<T>("to", Hash);
        else
            return GeneralizedModule.InvokeAsync<T>("to", Hash).GetAwaiter().GetResult();
    }
    public async ValueTask<T> ToAsync<T>()
    {
        if (IsWasm)
            return await Module.InvokeAsync<T>("to", Hash);
        else
            return await GeneralizedModule.InvokeAsync<T>("to", Hash);
    }

    public JObjPtr Construct(string classname, params object[] param)
    {
        JObjPtr ptr = new();
        var args = GetParamList(param);
        if (IsWasm)
            Module.InvokeVoid("construct", classname, args.AsSpan()[..param.Length].ToArray(), ptr.Hash, Hash);
        else
            GeneralizedModule.InvokeVoidAsync("construct", classname, args.AsSpan()[..param.Length].ToArray(), ptr.Hash, Hash)
                .GetAwaiter().GetResult();

        ParamPool.Return(args);
        return ptr;
    }
    public async ValueTask<JObjPtr> ConstructAsync(string classname, params object[] param)
    {
        JObjPtr ptr = new();
        var args = GetParamList(param);
        if (IsWasm)
            await Module.InvokeVoidAsync("construct", classname, args.AsSpan()[..param.Length].ToArray(), ptr.Hash, Hash);
        else
            await GeneralizedModule.InvokeVoidAsync("construct", classname, args.AsSpan()[..param.Length].ToArray(), ptr.Hash, Hash);

        ParamPool.Return(args);
        return ptr;
    }

    public void SetPropCallBack(string propname, Action<JObjPtr[]> action)
    {
        var cbk = new JCallback(action);
        if (IsWasm)
            Module.InvokeVoid("setcallback", propname, cbk.DotNet, Hash);
        else
            GeneralizedModule.InvokeVoidAsync("setcallback", propname, cbk.DotNet, Hash).GetAwaiter().GetResult();

    }
    public async ValueTask SetPropCallBackAsync(string propname, Action<JObjPtr[]> action)
    {
        var cbk = new JCallback(action);
        if (IsWasm)
            await Module.InvokeVoidAsync("setcallback", propname, cbk.DotNet, Hash);
        else
            await GeneralizedModule.InvokeVoidAsync("setcallback", propname, cbk.DotNet, Hash);

    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ParamInfo[] GetParamList(params object[] array)
    {
        var list = ParamPool.Rent(array.Length);
        var i = 0;
        foreach (var p in array)
        {
            if (p is JObjPtr ptr)
                list[i] = new() { Value = ptr.Hash, Type = ParamTypes.JObjPtr };
            else if (p is Action<JObjPtr[]> clbk)
                list[i] = new() { Type = ParamTypes.Callback, Value = (new JCallback(clbk).DotNet) };
            else
                list[i] = new() { Value = p };
            i++;
        }
        return list;
    }
    public JObjPtr this[string propertyname] => PropRef(propertyname);
    public override string ToString() => AsJsonText();

    public bool Equals(JObjPtr other)
    {
        if (IsWasm)
            return Module.Invoke<bool>("isEqualRef", other.Hash, Hash);
        else
            return GeneralizedModule.InvokeAsync<bool>("isEqualRef", other.Hash, Hash).GetAwaiter().GetResult();
    }
}