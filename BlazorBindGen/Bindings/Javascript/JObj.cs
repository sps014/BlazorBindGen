using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorBindGen
{
    public class JObj : IJavaScriptObject
    {
        internal int Hash { get; set; }
        internal static int HashTrack = 0;

        internal JObj()
        {
            Hash = HashTrack++;
        }
        ~JObj()
        {
            BindGen.Module.InvokeVoid("deleteprop", Hash);
        }

        public T Val<T>(string propname)
        {
            return BindGen.Module.Invoke<T>("propval", propname, Hash);
        }
        public async ValueTask<T> ValAsync<T>(string propname)
        {
            return await BindGen.Module.InvokeAsync<T>("propval", propname, Hash);
        }
        public JObj PropRef(string propname)
        {
            var obj = new JObj();
            BindGen.Module.InvokeVoid("propref", propname, obj.Hash, Hash);
            return obj;
        }
        public async ValueTask<JObj> PropRefAsync(string propname)
        {
            var obj = new JObj();
            await BindGen.Module.InvokeVoidAsync("propref", propname, obj.Hash, Hash);
            return obj;
        }

        public bool IsProp(string propname)
        {
            return BindGen.Module.Invoke<bool>("isprop", propname, Hash);
        }
        public bool IsFunc(string propname)
        {
            return BindGen.Module.Invoke<bool>("isfunc", propname, Hash);
        }

        public T Func<T>(string funcname, params object[] param)
        {
            return BindGen.Module.Invoke<T>("func", funcname, BindGen.GetParamList(param), Hash);
        }

        public async ValueTask<T> FuncAsync<T>(string funcname, params object[] param)
        {
            return await BindGen.Module.InvokeAsync<T>("func", funcname, BindGen.GetParamList(param), Hash);
        }

        public JObj FuncRef(string funcname, params object[] param)
        {
            JObj j = new();
            BindGen.Module.InvokeVoid("funcref", funcname, BindGen.GetParamList(param), j.Hash,Hash);
            return j;
        }

        public async ValueTask<JObj> FuncRefAsync(string funcname, params object[] param)
        {
            JObj j = new();
            await BindGen.Module.InvokeVoidAsync("funcref", funcname, BindGen.GetParamList(param), j.Hash,Hash);
            return j;
        }
        public void FuncVoid(string funcname, params object[] param)
        {
            BindGen.Module.InvokeVoid("funcvoid", funcname, BindGen.GetParamList(param), Hash);
        }

        public async void FuncVoidAsync(string funcname, params object[] param)
        {
            await BindGen.Module.InvokeVoidAsync("funcvoid", funcname, BindGen.GetParamList(param), Hash);
        }
        public JAwaitResult<T> FuncAwait<T>(string funcname, params object[] param)
        {
            return BindGen.Module.Invoke<JAwaitResult<T>>("funcawait", funcname, BindGen.GetParamList(param),Hash);
        }

        public async ValueTask<JAwaitResult<T>> FuncAwaitAsync<T>(string funcname, params object[] param)
        {
            return await BindGen.Module.InvokeAsync<JAwaitResult<T>>("funcawait", funcname, BindGen.GetParamList(param),Hash);
        }

        public JAwaitResult<JObj> FuncRefAwait(string funcname, params object[] param)
        {
            JObj obj = new();
            var err = BindGen.Module.Invoke<string>("funcrefawait", funcname, BindGen.GetParamList(param),Hash);
            return new() { Err = err, Value = obj };
        }

        public async ValueTask<JAwaitResult<JObj>> FuncRefAwaitAsync(string funcname, params object[] param)
        {
            JObj obj = new();
            var err = await BindGen.Module.InvokeAsync<string>("funcrefawait", funcname, BindGen.GetParamList(param),Hash);
            return new() { Err = err, Value = obj };
        }

        public string FuncVoidAwait(string funcname, params object[] param)
        {
            return BindGen.Module.Invoke<string>("funcrefvoidawait", funcname, BindGen.GetParamList(param),Hash);
        }

        public async ValueTask<string> FuncVoidAwaitAsync(string funcname, params object[] param)
        {
            return await BindGen.Module.InvokeAsync<string>("funcrefawait", funcname, BindGen.GetParamList(param),Hash);
        }
    }
}
