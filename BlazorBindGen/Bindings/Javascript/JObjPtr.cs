using Microsoft.JSInterop;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static BlazorBindGen.BindGen;

namespace BlazorBindGen
{
    public class JObjPtr:IEquatable<JObjPtr>
    {
        internal int Hash { get; private set; }
        private static int _hashTrack = 0;

        private static readonly ArrayPool<ParamInfo> ParamPool = ArrayPool<ParamInfo>.Shared;

        internal JObjPtr()
        {
            Hash = Interlocked.Increment(ref _hashTrack);
        }
        ~JObjPtr()
        {
            _ = Module.InvokeUnmarshalled<int, object>("deleteprop", Hash);
        }


        public T PropVal<T>(string propname) =>
            Module.Invoke<T>("propval", propname, Hash);
        public async ValueTask<T> PropValAsync<T>(string propname) =>
            await Module.InvokeAsync<T>("propval", propname, Hash);
        public JObjPtr PropRef(string propname)
        {
            JObjPtr obj = new();
            _ = Module.InvokeUnmarshalled<string, int, int, object>("propref", propname, obj.Hash, Hash);
            return obj;
        }
        public async ValueTask<JObjPtr> PropRefAsync(string propname)
        {
            JObjPtr obj = new();
            await Module.InvokeVoidAsync("propref", propname, obj.Hash, Hash);
            return obj;
        }
        public void SetPropVal<T>(string propname, T value) => 
            Module.InvokeVoid("propset", propname, value, Hash);

        public void SetPropRef(string propname, JObjPtr obj) => 
            _ = Module.InvokeUnmarshalled<string, int, int, object>("propsetref", propname, obj.Hash, Hash);

        public bool IsProp(string propname) => 
            Module.InvokeUnmarshalled<string, int, bool>("isprop", propname, Hash);
        public bool IsFunc(string propname) => 
            Module.InvokeUnmarshalled<string, int, bool>("isfunc", propname, Hash);

        public T Call<T>(string funcname, params object[] param)
        {
            var args = GetParamList(param);
            var res= Module.Invoke<T>("func", funcname, args, Hash);
            ParamPool.Return(args);
            return res;
        }

        public async ValueTask<T> CallAsync<T>(string funcname, params object[] param)
        {
            var args = GetParamList(param);
            var res= await Module.InvokeAsync<T>("func", funcname, args, Hash);
            ParamPool.Return(args);
            return res;
        }

        public JObjPtr CallRef(string funcname, params object[] param)
        {
            var args = GetParamList(param);
            JObjPtr j = new();
            Module.InvokeVoid("funcref", funcname,args, j.Hash,Hash);
            ParamPool.Return(args);
            return j;
        }

        public async ValueTask<JObjPtr> CallRefAsync(string funcname, params object[] param)
        {
            JObjPtr j = new();
            var args = GetParamList(param);
            await Module.InvokeVoidAsync("funcref", funcname, args, j.Hash,Hash);
            ParamPool.Return(args);
            return j;
        }
        public void CallVoid(string funcname, params object[] param)
        {
            var args = GetParamList(param);
            Module.InvokeVoid("funcvoid", funcname, args, Hash);
            ParamPool.Return(args);
        }

        public async void CallVoidAsync(string funcname, params object[] param)
        {
            var args = GetParamList(param);
            await Module.InvokeVoidAsync("funcvoid", funcname, args, Hash);
            ParamPool.Return(args);
        }

        public async ValueTask<JObjPtr> CallRefAwaitedAsync(string funcname, params object[] param)
        {
            JObjPtr obj = new();
            long errH = Interlocked.Increment(ref JCallBackHandler.ErrorTrack);
            var args = GetParamList(param);

            Module.InvokeVoid("funcrefawait", funcname, args, errH, obj.Hash,Hash);
            ParamPool.Return(args);

            await LockHandler.HoldVoid(errH);
            return obj;

        }

        public async ValueTask CallVoidAwaitedAsync(string funcname, params object[] param)
        {
            long errH = Interlocked.Increment(ref JCallBackHandler.ErrorTrack);
            var args = GetParamList(param);
            Module.InvokeVoid("funcvoidawait", funcname, args, errH,Hash);
            ParamPool.Return(args);

            await LockHandler.HoldVoid(errH);
        }

        public async ValueTask<T> CallAwaitedAsync<T>(string funcname, params object[] param)
        {
            long errH = Interlocked.Increment(ref JCallBackHandler.ErrorTrack);
            var args = GetParamList(param);

            Module.InvokeVoid("funcawait", funcname, args, errH, Hash);
            ParamPool.Return(args);

            return await LockHandler.Hold<T>(errH);

        }
        public string AsJsonText() => Module.Invoke<string>("asjson", Hash);
        public T To<T>() => Module.Invoke<T>("to", Hash);

        public JObjPtr Construct(string classname, params object[] param)
        {
            JObjPtr ptr = new();
            var args = GetParamList(param);
            Module.InvokeVoid("construct", classname, args,ptr.Hash,Hash);
            ParamPool.Return(args);
            return ptr;
        }

        public void SetPropCallBack(string propname, Action<JObjPtr[]> action)
        {
            var cbk = new JCallback(action);
            Module.InvokeVoid("setcallback",propname, cbk.DotNet, Hash);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ParamInfo[] GetParamList(params object[] array)
        {
            var list = ParamPool.Rent(array.Length);
            int i = 0;
            foreach (var p in array)
            {
                if (p is JObjPtr)
                {
                    list[i] = new() { Value = (p as JObjPtr).Hash, Type = ParamTypes.JOBJ };
                }
                else if (p is Action<JObjPtr[]>)
                {
                    list[i] = new() { Type = ParamTypes.CALLBACK, Value = (new JCallback(p as Action<JObjPtr[]>)).DotNet };
                }
                else
                {
                    list[i] = new() { Value = p };
                }
                i++;
            }
            return list;
        }
        public JObjPtr this[string propertyname]=>PropRef(propertyname);
        public override string ToString() => AsJsonText();

        public bool Equals(JObjPtr other) => Module.Invoke<bool>("isEqualRef", other.Hash, Hash);
    }
}
