using Microsoft.JSInterop;
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
            return BindGen.Module.Invoke<T>("funcwin", funcname, param);
        }

        public async ValueTask<T> FuncAsync<T>(string funcname, params object[] param)
        {
            return await BindGen.Module.InvokeAsync<T>("funcwin", funcname, param);
        }

        public JObj FuncRef(string funcname, params object[] param)
        {
            JObj j= new();
            BindGen.Module.InvokeVoid("funcrefwin", funcname, param,j.Hash);
            return j;
        }

        public async ValueTask<JObj> FuncRefAsync(string funcname, params object[] param)
        {
            JObj j = new();
            await BindGen.Module.InvokeVoidAsync("funcrefwin", funcname, param, j.Hash);
            return j;
        }
    
    }
}
