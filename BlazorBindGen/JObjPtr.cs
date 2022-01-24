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
public class JObjPtr : IEquatable<JObjPtr?>
{
    /// <summary>
    /// Current Hash of JS Object used for Memory Management 
    /// </summary>
    internal int Hash { get; private set; }

    /// <summary>
    /// Internal JS Object Pointer Address Allocator (Counter)
    /// </summary>
    private static int _hashCount;

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
            _ = Module.InvokeUnmarshalled<int, object>("DeletePtr", Hash);
        else
            GeneralizedModule.InvokeVoidAsync("DeletePtr", Hash);
    }

    /// <summary>
    /// Get Exact C# Serializable Property Value from JS Object
    /// <para>Equivalent to (obj.prop)</para>
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
    /// <summary>
    /// Get Exact C# Serializable Property Value from JS Object (Supported in WASM and Server)
    /// <para>Equivalent to (obj.prop)</para>
    /// </summary>
    /// <param name="propertyName">Name of the property to get</param>
    /// <typeparam name="T">.net convertable type</typeparam>
    /// <returns>exact value of property</returns>
    public async ValueTask<T> PropValAsync<T>(string propertyName)
    {
        if (IsWasm)
            return await Module.InvokeAsync<T>("PropVal", propertyName, Hash);
        else
            return await GeneralizedModule.InvokeAsync<T>("PropVal", propertyName, Hash);
    }
    /// <summary>
    /// Get Reference Pointer to JS Object Property in WASM
    /// <para>Equivalent to let objPtr=obj.prop</para>
    /// </summary>
    /// <param name="propertyName">Name of the property to get </param>
    /// <returns>Pointer to that property</returns>
    public JObjPtr PropRef(string propertyName)
    {
        JObjPtr obj = new();
        if (IsWasm)
            _ = Module.InvokeUnmarshalled<string, int, int, object>("PropRef", propertyName, obj.Hash, Hash);
        else
            throw PlatformUnsupportedException.Throw();

        return obj;
    }
    /// <summary>
    /// Get Reference Pointer to JS Object Property in WASM and Server
    /// <para>Equivalent to let objPtr=obj.prop</para>
    /// </summary>
    /// <param name="propertyName">Name of the property to get </param>
    /// <returns>Pointer to that property</returns>
    public async ValueTask<JObjPtr> PropRefAsync(string propertyName)
    {
        JObjPtr obj = new();
        if (IsWasm)
            _ = Module.InvokeUnmarshalled<string, int, int, object>("PropRef", propertyName, obj.Hash, Hash);
        else
            await GeneralizedModule.InvokeVoidAsync("PropRefGen", propertyName, obj.Hash, Hash);
        return obj;
    }
    /// <summary>
    /// Set Exact C# Serializable Value on JS Object Property in WASM
    /// <para>equivalent to eg. obj.prop=value</para>
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value">C# Serializable Value to set to JS object property</param>
    /// <typeparam name="T"></typeparam>
    public void SetPropVal<T>(string propertyName, T value)
    {
        if (IsWasm)
            Module.InvokeVoid("PropSet", propertyName, value, Hash);
        else
            throw PlatformUnsupportedException.Throw();
    }

    /// <summary>
    /// Set a JObjPtr as Property of  JS Object  in WASM 
    /// <para>equivalent to eg. obj.prop=obj2</para>
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="obj">JObjPtr</param>
    /// <exception cref="PlatformUnsupportedException"></exception>
    public void SetPropRef(string propertyName, JObjPtr obj)
    {
        if (IsWasm)
            _ = Module.InvokeUnmarshalled<string, int, int, object>("PropSetRef", propertyName, obj.Hash, Hash);
        else
            throw PlatformUnsupportedException.Throw();
    }

    /// <summary>
    /// Set a JObjPtr as Property of  JS Object  in WASM and Server
    /// <para>equivalent to eg. obj.prop=obj2</para>
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="obj"></param>
    public async ValueTask SetPropRefAsync(string propertyName, JObjPtr obj)
    {
        if (IsWasm)
            _ = Module.InvokeUnmarshalled<string, int, int, object>("PropSetRef", propertyName, obj.Hash, Hash);
        else
            await GeneralizedModule.InvokeVoidAsync("PropSetRefGen", propertyName, obj.Hash, Hash);
    }

    /// <summary>
    /// Check whether property with the given name exists in JS Object in WASM
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    /// <exception cref="PlatformUnsupportedException"></exception>
    public bool IsProp(string propertyName)
    {
        if (IsWasm)
            return Module.InvokeUnmarshalled<string, int, bool>("IsProp", propertyName, Hash);
        else
            throw PlatformUnsupportedException.Throw();
    }
    /// <summary>
    /// Check whether property with the given name exists in JS Object in WASM and Server
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public async ValueTask<bool> IsPropAsync(string propertyName)
    {
        if (IsWasm)
            return Module.InvokeUnmarshalled<string, int, bool>("IsProp", propertyName, Hash);
        else
            return await GeneralizedModule.InvokeAsync<bool>("IsPropGen", propertyName, Hash);
    }

    /// <summary>
    /// Check whether function with the given name exists in JS Object in WASM
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    /// <exception cref="PlatformUnsupportedException"></exception>
    public bool IsFunc(string propertyName)
    {
        if (IsWasm)
            return Module.InvokeUnmarshalled<string, int, bool>("IsFunc", propertyName, Hash);
        else
            throw PlatformUnsupportedException.Throw();
    }
    /// <summary>
    /// Check whether function with the given name exists in JS Object in WASM and Server
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public async ValueTask<bool> IsFuncAsync(string propertyName)
    {
        if (IsWasm)
            return Module.InvokeUnmarshalled<string, int, bool>("IsFunc", propertyName, Hash);
        else
            return await GeneralizedModule.InvokeAsync<bool>("IsFuncGen", propertyName, Hash);
    }

    /// <summary>
    /// Call a function with the given name in JS Object in WASM
    /// <para>equivalent to obj.func(param1,param2)</para>
    /// </summary>
    /// <param name="funcName">Name of the Function</param>
    /// <param name="param">Parameter to the function</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>.NET Serializable Value</returns>
    /// <exception cref="PlatformUnsupportedException"></exception>
    public T Call<T>(string funcName, params object[] param)
    {
        var args = GetParamList(param);
        T res;
        if (IsWasm)
            res = Module.Invoke<T>("Func", funcName, args.AsSpan()[..param.Length].ToArray(), Hash);
        else
            throw PlatformUnsupportedException.Throw();

        ParamPool.Return(args);
        return res;
    }
    /// <summary>
    /// Call a function with the given name in JS Object in WASM and Server
    /// <para>equivalent to obj.func(param1,param2)</para>
    /// </summary>
    /// <param name="funcName"></param>
    /// <param name="param"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async ValueTask<T> CallAsync<T>(string funcName, params object[] param)
    {
        var args = GetParamList(param);
        T res;
        if (IsWasm)
            res = await Module.InvokeAsync<T>("Func", funcName, args.AsSpan()[..param.Length].ToArray(), Hash);
        else
            res = await GeneralizedModule.InvokeAsync<T>("Func", funcName, args.AsSpan()[..param.Length].ToArray(), Hash);

        ParamPool.Return(args);
        return res;
    }
    /// <summary>
    /// Call a function with the given name in JS Object that return Ptr as result in WASM 
    /// <para>equivalent to let objPtr=obj.func(param1,param2)</para>
    /// </summary>
    /// <param name="funcName"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    /// <exception cref="PlatformUnsupportedException"></exception>
    public JObjPtr CallRef(string funcName, params object[] param)
    {
        var args = GetParamList(param);
        JObjPtr j = new();
        if (IsWasm)
            Module.InvokeVoid("FuncRef", funcName, args.AsSpan()[..param.Length].ToArray(), j.Hash, Hash);
        else
            throw PlatformUnsupportedException.Throw();
        ParamPool.Return(args);
        return j;
    }
    /// <summary>
    /// Call a function with the given name in JS Object that return Ptr as result in WASM and Server
    /// <para>equivalent to let objPtr=obj.func(param1,param2)</para>
    /// </summary>
    /// <param name="funcName"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public async ValueTask<JObjPtr> CallRefAsync(string funcName, params object[] param)
    {
        JObjPtr j = new();
        var args = GetParamList(param);
        if (IsWasm)
            await Module.InvokeVoidAsync("FuncRef", funcName, args.AsSpan()[..param.Length].ToArray(), j.Hash, Hash);
        else
            await GeneralizedModule.InvokeVoidAsync("FuncRef", funcName, args.AsSpan()[..param.Length].ToArray(), j.Hash, Hash);
        ParamPool.Return(args);
        return j;
    }

    /// <summary>
    /// Call a function with the given name in JS Object that returns void  (WASM)
    /// <para>equivalent to obj.func(param1)</para>
    /// </summary>
    /// <param name="funcName"></param>
    /// <param name="param"></param>
    /// <exception cref="PlatformUnsupportedException"></exception>
    public void CallVoid(string funcName, params object[] param)
    {
        var args = GetParamList(param);
        if (IsWasm)
            Module.InvokeVoid("FuncVoid", funcName, args.AsSpan()[..param.Length].ToArray(), Hash);
        else
            throw PlatformUnsupportedException.Throw();
        ParamPool.Return(args);
    }
    /// <summary>
    /// Call a function with the given name in JS Object that returns void ( WASM and Server )
    /// <para>equivalent to obj.func(param1)</para>
    /// </summary>
    /// <param name="funcName"></param>
    /// <param name="param"></param>
    public async ValueTask CallVoidAsync(string funcName, params object[] param)
    {
        var args = GetParamList(param);
        if (IsWasm)
            await Module.InvokeVoidAsync("FuncVoid", funcName, args.AsSpan()[..param.Length].ToArray(), Hash);
        else
            await GeneralizedModule.InvokeVoidAsync("FuncVoid", funcName, args.AsSpan()[..param.Length].ToArray(), Hash);
        ParamPool.Return(args);
    }

    /// <summary>
    /// Call a async JS function  that will return pointer as awaited result with the given name in JS Object
    /// <para>Equivalent to let c=await obj.func(param,param2)</para>
    /// </summary>
    /// <param name="funcName"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public async ValueTask<JObjPtr> CallRefAwaitedAsync(string funcName, params object[] param)
    {
        JObjPtr obj = new();
        long errH = Interlocked.Increment(ref JCallBackHandler.SyncCounter);
        var args = GetParamList(param);
        if (IsWasm)
            Module.InvokeVoid("FuncRefAwait", funcName, args.AsSpan()[..param.Length].ToArray(), errH, obj.Hash, Hash);
        else
            await GeneralizedModule.InvokeVoidAsync("FuncRefAwait", funcName, args.AsSpan()[..param.Length].ToArray(), errH, obj.Hash, Hash);

        ParamPool.Return(args);

        await LockHandler.HoldVoid(errH);
        return obj;

    }
    /// <summary>
    /// Call a async JS function with the given name in JS Object
    /// <para>Equivalent to await obj.func(param,param2)</para>
    /// </summary>
    /// <param name="funcName"></param>
    /// <param name="param"></param>

    public async ValueTask CallVoidAwaitedAsync(string funcName, params object[] param)
    {
        long errH = Interlocked.Increment(ref JCallBackHandler.SyncCounter);
        var args = GetParamList(param);
        if (IsWasm)
            Module.InvokeVoid("FuncVoidAwait", funcName, args.AsSpan()[..param.Length].ToArray(), errH, Hash);
        else
            await GeneralizedModule.InvokeVoidAsync("FuncVoidAwait", funcName, args.AsSpan()[..param.Length].ToArray(), errH, Hash);

        ParamPool.Return(args);
        await LockHandler.HoldVoid(errH);
    }

    /// <summary>
    /// Call a async JS function  that will return C# Serializable value as awaited result with the given name in JS Object
    /// </summary>
    /// <param name="funcName"></param>
    /// <param name="param"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async ValueTask<T?> CallAwaitedAsync<T>(string funcName, params object[] param)
    {
        long errH = Interlocked.Increment(ref JCallBackHandler.SyncCounter);
        var args = GetParamList(param);
        if (IsWasm)
            Module.InvokeVoid("FuncAwait", funcName, args.AsSpan()[..param.Length].ToArray(), errH, Hash);
        else
            await GeneralizedModule.InvokeVoidAsync("FuncAwait", funcName, args.AsSpan()[..param.Length].ToArray(), errH, Hash);

        ParamPool.Return(args);
        return await LockHandler.Hold<T>(errH);
    }

    /// <summary>
    /// Return JSON string Representation of object in WASM
    /// </summary>
    /// <returns></returns>
    public string AsJsonText()
    {
        if (IsWasm)
            return Module.Invoke<string>("AsJson", Hash);
        else
            throw PlatformUnsupportedException.Throw();

    }
    /// <summary>
    /// Return JSON string representation of object in WASM and server
    /// </summary>
    /// <returns></returns>
    public async ValueTask<string> AsJsonTextAsync()
    {
        if (IsWasm)
            return await Module.InvokeAsync<string>("AsJson", Hash);
        else
            return await GeneralizedModule.InvokeAsync<string>("AsJson", Hash);
    }
    /// <summary>
    /// Convert JS Object to C# Object in WASM
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="PlatformUnsupportedException"></exception>
    public T To<T>()
    {
        if (IsWasm)
            return Module.Invoke<T>("To", Hash);
        else
            throw PlatformUnsupportedException.Throw();
    }
    /// <summary>
    /// Convert JS Object to C# Object in WASM and server
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async ValueTask<T> ToAsync<T>()
    {
        if (IsWasm)
            return await Module.InvokeAsync<T>("To", Hash);
        else
            return await GeneralizedModule.InvokeAsync<T>("To", Hash);
    }
    /// <summary>
    /// Create instance of JS Object from current object
    /// <para>let c=new obj.Dom(params)</para>
    /// </summary>
    /// <param name="className"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public JObjPtr Construct(string className, params object[] param)
    {
        JObjPtr ptr = new();
        var args = GetParamList(param);
        if (IsWasm)
            Module.InvokeVoid("Construct", className, args.AsSpan()[..param.Length].ToArray(), ptr.Hash, Hash);
        else
            throw PlatformUnsupportedException.Throw();

        ParamPool.Return(args);
        return ptr;
    }
    /// <summary>
    /// Create instance of JS Object from current object
    /// <para>let c=new obj.Dom(params)</para>
    /// </summary>
    /// <param name="className"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public async ValueTask<JObjPtr> ConstructAsync(string className, params object[] param)
    {
        JObjPtr ptr = new();
        var args = GetParamList(param);
        if (IsWasm)
            await Module.InvokeVoidAsync("Construct", className, args.AsSpan()[..param.Length].ToArray(), ptr.Hash, Hash);
        else
            await GeneralizedModule.InvokeVoidAsync("Construct", className, args.AsSpan()[..param.Length].ToArray(), ptr.Hash, Hash);

        ParamPool.Return(args);
        return ptr;
    }
    /// <summary>
    /// Subscribe to JS Events in WASM
    /// <para>equivalent to audio.onloadmetadata=methodDelegate;</para>
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="action">Method to be executed on event callback</param>
    /// <exception cref="PlatformUnsupportedException"></exception>
    public void SetPropCallBack(string propertyName, Action<JObjPtr[]> action)
    {
        var cbk = new JCallback(action);
        if (IsWasm)
            Module.InvokeVoid("SetCallback", propertyName, cbk.DotNet, Hash);
        else
            throw PlatformUnsupportedException.Throw();
    }
    /// <summary>
    /// Subscribe to JS Events in WASM and server
    /// <para>equivalent to audio.onloadmetadata=methodDelegate;</para>
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="action">Method to be executed on event callback</param>
    public async ValueTask SetPropCallBackAsync(string propertyName, Action<JObjPtr[]> action)
    {
        var cbk = new JCallback(action);
        if (IsWasm)
            await Module.InvokeVoidAsync("SetCallback", propertyName, cbk.DotNet, Hash);
        else
            await GeneralizedModule.InvokeVoidAsync("SetCallback", propertyName, cbk.DotNet, Hash);

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
            else if (p is Action<JObjPtr[]> callback)
                list[i] = new() { Type = ParamTypes.Callback, Value = (new JCallback(callback).DotNet) };
            else
                list[i] = new() { Value = p };
            i++;
        }
        return list;
    }
    /// <summary>
    /// Get JS Object property Pointer (works Web Assembly only , throws platform not supported)
    /// <para>eg. let doc=window.document</para>
    /// </summary>
    /// <param name="propertyName">[web assembly only] property name whose reference you want to fetch</param>
    public JObjPtr this[string propertyName] => PropRef(propertyName);
    public override string ToString() => AsJsonText();

    public bool Equals(JObjPtr? other)
    {
        if (other == null)
            return false;

        if (IsWasm)
            return Module.Invoke<bool>("isEqualRef", other.Hash, Hash);
        else
            return GeneralizedModule.InvokeAsync<bool>("isEqualRef", other.Hash, Hash).GetAwaiter().GetResult();
    }
    /// <summary>
    /// Check equality of two object pointers
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public async ValueTask<bool> EqualsAsync(JObjPtr? other)
    {
        if (other == null)
            return false;

        if (IsWasm)
            return Module.Invoke<bool>("isEqualRef", other.Hash, Hash);
        else
            return await GeneralizedModule.InvokeAsync<bool>("isEqualRef", other.Hash, Hash);
    }

    /// <summary>
    /// console.log current object
    /// </summary>
    /// <returns></returns>
    public async Task LogAsync()
    {
        if (IsWasm)
            await Module.InvokeVoidAsync("logPtr", Hash);
        else
            await GeneralizedModule.InvokeVoidAsync("logPtr", Hash);
    }
    /// <summary>
    /// console.log current object
    /// </summary>
    /// <returns></returns>
    public void Log()
    {
        if (IsWasm)
             Module.InvokeVoid("logPtr", Hash);
        else
            PlatformUnsupportedException.Throw();
    }
}