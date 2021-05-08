using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBindGen
{
    public sealed class JWindow : IJObj
    {

        private JWindow()
        {

        }
        ~JWindow()
        {
            //dispose
        }
        internal static JWindow CreateJWindowObject()
        {
            return new JWindow();
        }

        public T Prop<T>(string propname)
        {
            return BindGen.Module.Invoke<T>("propwin", propname);
        }
        public async ValueTask<T> PropAsync<T>(string propname)
        {
            return await BindGen.Module.InvokeAsync<T>("propwin", propname);
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

    }
}
