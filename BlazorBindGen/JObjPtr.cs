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
    internal Guid Hash { get; private set; }

    /// <summary>
    /// Internal JS Object Pointer Address Allocator (Counter)
    /// </summary>
    private static Guid _hashCount;

    /// <summary>
    /// Internal Constructor helps in getting new hash address
    /// </summary>
    /// 
    private readonly static Lock _lockObj = new(); // Initialize the new Lock object


    internal JObjPtr()
    {

        _lockObj.EnterScope();
        Hash = Guid.CreateVersion7();
        _lockObj.Exit();
    }
    ~JObjPtr()
    {
        if (IsWasm)
            WasmModule.InvokeVoid("DeletePtr", Hash);
        else
            ServerModule.InvokeVoidAsync("DeletePtr", Hash);
    }

    /// <summary>
    /// Checks if current ptr is undefined or null (On WASM)
    /// </summary>
    /// <returns></returns>
    public bool IsNullOrUndefined()
    {
        if (IsWasm)
            return WasmModule.Invoke<bool>("IsNullOrUndefined", Hash);
        else
            throw PlatformUnsupportedException.Throw();
    }

    /// <summary>
    /// Checks if current ptr is undefined or null 
    /// </summary>
    /// <returns></returns>
    public ValueTask<bool> IsNullOrUndefinedAsync()
    {
        return CommonModule.InvokeAsync<bool>("IsNullOrUndefined", Hash);
    }

    public string[] GetAllProperties()
    {
        if (IsWasm)
            return WasmModule.Invoke<string[]>("GetAllProperties", Hash);
        else
            throw PlatformUnsupportedException.Throw();
    }

    public ValueTask<string[]> GetAllPropertiesAsync()
    {
        return CommonModule.InvokeAsync<string[]>("GetAllProperties", Hash);
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
            return WasmModule.Invoke<T>("PropVal", propertyName, Hash);
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
        return await CommonModule.InvokeAsync<T>("PropVal", propertyName, Hash);
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
            WasmModule.InvokeVoid("PropRef", propertyName, obj.Hash, Hash);
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
        await CommonModule.InvokeVoidAsync("PropRef", propertyName, obj.Hash, Hash);
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
            WasmModule.InvokeVoid("PropSet", propertyName, value, Hash);
        else
            throw PlatformUnsupportedException.Throw();
    }
    /// <summary>
    /// Set Exact C# Serializable Value on JS Object Property in WASM and server
    /// <para>equivalent to eg. obj.prop=value</para>
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value">C# Serializable Value to set to JS object property</param>
    /// <typeparam name="T"></typeparam>
    public ValueTask SetPropValAsync<T>(string propertyName, T value)
    {
        return CommonModule.InvokeVoidAsync("PropSet", propertyName, value, Hash);
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
            WasmModule.InvokeVoid("PropSetRef", propertyName, obj.Hash, Hash);
        else
            throw PlatformUnsupportedException.Throw();
    }

    /// <summary>
    /// Set a JObjPtr as Property of  JS Object  in WASM and Server
    /// <para>equivalent to eg. obj.prop=obj2</para>
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="obj"></param>
    public  ValueTask SetPropRefAsync(string propertyName, JObjPtr obj)
    {
        return CommonModule.InvokeVoidAsync("PropSetRef", propertyName, obj.Hash, Hash);
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
            return WasmModule.Invoke<bool>("IsProp", propertyName, Hash);
        else
            throw PlatformUnsupportedException.Throw();
    }
    /// <summary>
    /// Check whether property with the given name exists in JS Object in WASM and Server
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public ValueTask<bool> IsPropAsync(string propertyName)
    {
        return  CommonModule.InvokeAsync<bool>("IsProp", propertyName, Hash);
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
            return WasmModule.Invoke<bool>("IsFunc", propertyName, Hash);
        else
            throw PlatformUnsupportedException.Throw();
    }
    /// <summary>
    /// Check whether function with the given name exists in JS Object in WASM and Server
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public ValueTask<bool> IsFuncAsync(string propertyName)
    {
        return CommonModule.InvokeAsync<bool>("IsFunc", propertyName, Hash);
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
            res = WasmModule.Invoke<T>("Func", funcName, args, Hash);
        else
            throw PlatformUnsupportedException.Throw();

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
        T res = await CommonModule.InvokeAsync<T>("Func", funcName, args, Hash);
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
            WasmModule.InvokeVoid("FuncRef", funcName, args, j.Hash, Hash);
        else
            throw PlatformUnsupportedException.Throw();
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
        await CommonModule.InvokeVoidAsync("FuncRef", funcName, args, j.Hash, Hash);
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
            WasmModule.InvokeVoid("FuncVoid", funcName, args, Hash);
        else
            throw PlatformUnsupportedException.Throw();
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
        await CommonModule.InvokeVoidAsync("FuncVoid", funcName, args, Hash);
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
        #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            CommonModule.InvokeVoidAsync("FuncRefAwait", funcName, args, errH, obj.Hash, Hash).ConfigureAwait(false);
        #pragma warning restore CS4014 

        await LockHandler.HoldVoid(errH);
        return obj;

    }
    /// <summary>
    /// Call a async JS function with the given name in JS Object
    /// <para>Equivalent to await obj.func(param,param2)</para>
    /// </summary>
    /// <param name="funcName"></param>
    /// <param name="param"></param>

    public ValueTask CallVoidAwaitedAsync(string funcName, params object[] param)
    {
        long errH = Interlocked.Increment(ref JCallBackHandler.SyncCounter);
        var args = GetParamList(param);
        CommonModule.InvokeVoidAsync("FuncVoidAwait", funcName, args, errH, Hash).ConfigureAwait(false);
        return LockHandler.HoldVoid(errH);
    }

    /// <summary>
    /// Call a async JS function  that will return C# Serializable value as awaited result with the given name in JS Object
    /// </summary>
    /// <param name="funcName"></param>
    /// <param name="param"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public ValueTask<T?> CallAwaitedAsync<T>(string funcName, params object[] param)
    {
        long errH = Interlocked.Increment(ref JCallBackHandler.SyncCounter);
        var args = GetParamList(param);
        CommonModule.InvokeVoidAsync("FuncAwait", funcName, args, errH, Hash).ConfigureAwait(false);
        return LockHandler.Hold<T>(errH);
    }

    /// <summary>
    /// Return JSON string Representation of object in WASM
    /// </summary>
    /// <returns></returns>
    public string AsJsonText()
    {
        if (IsWasm)
            return WasmModule.Invoke<string>("AsJson", Hash);
        else
            throw PlatformUnsupportedException.Throw();

    }
    /// <summary>
    /// Return JSON string representation of object in WASM and server
    /// </summary>
    /// <returns></returns>
    public  ValueTask<string> AsJsonTextAsync()
    {
        return CommonModule.InvokeAsync<string>("AsJson", Hash);
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
            return WasmModule.Invoke<T>("To", Hash);
        else
            throw PlatformUnsupportedException.Throw();
    }
    /// <summary>
    /// Convert JS Object to C# Object in WASM and server
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public ValueTask<T> ToAsync<T>()
    {
         return CommonModule.InvokeAsync<T>("To", Hash);
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
            WasmModule.InvokeVoid("Construct", className, args, ptr.Hash, Hash);
        else
            throw PlatformUnsupportedException.Throw();
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
        await CommonModule.InvokeVoidAsync("Construct", className, args, ptr.Hash, Hash);
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
            WasmModule.InvokeVoid("SetCallback", propertyName, cbk.DotNet, Hash);
        else
            throw PlatformUnsupportedException.Throw();
    }
    /// <summary>
    /// Subscribe to JS Events in WASM and server
    /// <para>equivalent to audio.onloadmetadata=methodDelegate;</para>
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="action">Method to be executed on event callback</param>
    public  ValueTask SetPropCallBackAsync(string propertyName, Action<JObjPtr[]> action)
    {
        var cbk = new JCallback(action);
        return CommonModule.InvokeVoidAsync("SetCallback", propertyName, cbk.DotNet, Hash);

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ParamInfo[] GetParamList(params object[] array)
    {
        var list = new ParamInfo[array.Length];
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
            return WasmModule.Invoke<bool>("isEqualRef", other.Hash, Hash);
        else
            return ServerModule.InvokeAsync<bool>("isEqualRef", other.Hash, Hash).GetAwaiter().GetResult();
    }
    /// <summary>
    /// Check equality of two object pointers
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public ValueTask<bool> EqualsAsync(JObjPtr? other)
    {
        if (other == null)
            return ValueTask.FromResult(false);

         return CommonModule.InvokeAsync<bool>("isEqualRef", other.Hash, Hash);
    }

    /// <summary>
    /// console.log current object
    /// </summary>
    /// <returns></returns>
    public ValueTask LogAsync()
    {
        return CommonModule.InvokeVoidAsync("logPtr", Hash);
    }
    /// <summary>
    /// console.log current object
    /// </summary>
    /// <returns></returns>
    public void Log()
    {
        if (IsWasm)
            WasmModule.InvokeVoid("logPtr", Hash);
        else
            PlatformUnsupportedException.Throw();
    }
}