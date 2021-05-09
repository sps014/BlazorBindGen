using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace BlazorBindGen
{
    public sealed class JWindow : IJavaScriptObject
    {

        private JWindow() { }
 
        internal static JWindow CreateJWindowObject()
        {
            return new JWindow();
        }

        public T Val<T>(string propname)
        {
            return BindGen.Module.Invoke<T>("propvalwin", propname);
        }
        public async ValueTask<T> ValAsync<T>(string propname)
        {
            return await BindGen.Module.InvokeAsync<T>("propvalwin", propname);
        }
        public JObj PropRef(string propname)
        {
            var obj = new JObj();
            BindGen.Module.InvokeVoid("proprefwin", propname, obj.Hash);
            return obj;
        }
        public async ValueTask<JObj> PropRefAsync(string propname)
        {
            var obj = new JObj();
            await BindGen.Module.InvokeVoidAsync("proprefwin", propname, obj.Hash);
            return obj;
        }
        public void SetVal<T>(string propname,T value)
        {
            BindGen.Module.InvokeVoid("propsetwin", propname, value);
        }

        public void SetPropRef(string propname,JObj obj)
        {
            BindGen.Module.InvokeVoid("propsetrefwin", propname, obj.Hash);
        }

        public bool IsFunc(string propname)
        {
            return BindGen.Module.Invoke<bool>("isfuncwin", propname);
        }

        public bool IsProp(string propname)
        {
            return BindGen.Module.Invoke<bool>("ispropwin", propname);
        }

        public T Func<T>(string funcname, params object[] param)
        {
            return BindGen.Module.Invoke<T>("funcwin",funcname, BindGen.GetParamList(param));
        }

        public async ValueTask<T> FuncAsync<T>(string funcname, params object[] param)
        {
            return await BindGen.Module.InvokeAsync<T>("funcwin", funcname, BindGen.GetParamList(param));
        }

        public JObj FuncRef(string funcname, params object[] param)
        {
            JObj j= new();
            BindGen.Module.InvokeVoid("funcrefwin", funcname, BindGen.GetParamList(param), j.Hash);
            return j;
        }

        public async ValueTask<JObj> FuncRefAsync(string funcname, params object[] param)
        {
            JObj j = new();
            await BindGen.Module.InvokeVoidAsync("funcrefwin", funcname, BindGen.GetParamList(param), j.Hash);
            return j;
        }

        public void FuncVoid(string funcname, params object[] param)
        {
            BindGen.Module.InvokeVoid("funcvoidwin", funcname, BindGen.GetParamList(param));
        }

        public async void FuncVoidAsync(string funcname, params object[] param)
        {
            await BindGen.Module.InvokeVoidAsync("funcvoidwin", funcname, BindGen.GetParamList(param));
        }

        public JAwaitResult<T> FuncAwait<T>(string funcname, params object[] param)
        {
            return BindGen.Module.Invoke<JAwaitResult<T>>("funcawaitwin", funcname, BindGen.GetParamList(param));
        }

        public async ValueTask<JAwaitResult<T>> FuncAwaitAsync<T>(string funcname, params object[] param)
        {
            return await BindGen.Module.InvokeAsync<JAwaitResult<T>>("funcawaitwin", funcname, BindGen.GetParamList(param));
        }

        public JAwaitResult<JObj> FuncRefAwait(string funcname, params object[] param)
        {
            
                JObj obj = new();
                BindGen.Module.InvokeVoid("funcrefawaitwin", funcname, BindGen.GetParamList(param));
                return new() { Err = "", Value = obj };
           

        }

        public async ValueTask<JAwaitResult<JObj>> FuncRefAwaitAsync(string funcname, params object[] param)
        {
            JObj obj = new();
            var err = await BindGen.Module.InvokeAsync<string>("funcrefawaitwin", funcname, BindGen.GetParamList(param));
            return new() { Err = err, Value = obj };
        }

        public string FuncVoidAwait(string funcname, params object[] param)
        {
            return BindGen.Module.Invoke<string>("funcrefvoidawaitwin", funcname, BindGen.GetParamList(param));
        }

        public async ValueTask<string>  FuncVoidAwaitAsync(string funcname, params object[] param)
        {
            return await BindGen.Module.InvokeAsync<string>("funcrefawaitwin", funcname, BindGen.GetParamList(param));
        }

        
    }
}
