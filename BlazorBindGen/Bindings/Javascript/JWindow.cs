using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
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
        public JObjPtr PropRef(string propname)
        {
            var obj = new JObjPtr();
            BindGen.Module.InvokeVoid("proprefwin", propname, obj.Hash);
            return obj;
        }
        public async ValueTask<JObjPtr> PropRefAsync(string propname)
        {
            var obj = new JObjPtr();
            await BindGen.Module.InvokeVoidAsync("proprefwin", propname, obj.Hash);
            return obj;
        }
        public void SetVal<T>(string propname,T value)
        {
            BindGen.Module.InvokeVoid("propsetwin", propname, value);
        }

        public void SetPropRef(string propname,JObjPtr obj)
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
            var args = BindGen.GetParamList(param);

            var r= BindGen.Module.Invoke<T>("funcwin",funcname, args);
            BindGen.ParamPool.Return(args);

            return r;
        }

        public async ValueTask<T> FuncAsync<T>(string funcname, params object[] param)
        {
            var args = BindGen.GetParamList(param);
            var r= await BindGen.Module.InvokeAsync<T>("funcwin", funcname, args);
            BindGen.ParamPool.Return(args);
            return r;

        }

        public JObjPtr FuncRef(string funcname, params object[] param)
        {
            JObjPtr j= new();
            var args = BindGen.GetParamList(param);
            BindGen.Module.InvokeVoid("funcrefwin", funcname, args, j.Hash);
            BindGen.ParamPool.Return(args);
            return j;
        }

        public async ValueTask<JObjPtr> FuncRefAsync(string funcname, params object[] param)
        {
            JObjPtr j = new();
            var args = BindGen.GetParamList(param);
            await BindGen.Module.InvokeVoidAsync("funcrefwin", funcname, args, j.Hash);
            BindGen.ParamPool.Return(args);
            return j;
        }

        public void FuncVoid(string funcname, params object[] param)
        {
            var args = BindGen.GetParamList(param);
            BindGen.Module.InvokeVoid("funcvoidwin", funcname, args);
            BindGen.ParamPool.Return(args);

        }

        public async void FuncVoidAsync(string funcname, params object[] param)
        {
            var args = BindGen.GetParamList(param);
            await BindGen.Module.InvokeVoidAsync("funcvoidwin", funcname, args);
            BindGen.ParamPool.Return(args);

        }



        public async ValueTask<JObjPtr> FuncRefAwaitAsync(string funcname, params object[] param)
        {
            
            JObjPtr obj = new();
            long errH = Interlocked.Increment(ref JCallBackHandler.ErrorTrack);
            var args = BindGen.GetParamList(param);

            BindGen.Module.InvokeVoid("funcrefawaitwin", funcname, args, errH, obj.Hash);
            BindGen.ParamPool.Return(args);

            (object,string) tpl;
            while (!JCallBackHandler.ErrorMessages.TryGetValue(errH, out _))
            {
                await Task.Delay(5);
            }
            JCallBackHandler.ErrorMessages.TryRemove(errH, out tpl);
            if (!string.IsNullOrWhiteSpace(tpl.Item2))
                throw new Exception(tpl.Item2);

            return obj;
           

        }

        public async ValueTask FuncVoidAwaitAsync(string funcname, params object[] param)
        {
            long errH = Interlocked.Increment(ref JCallBackHandler.ErrorTrack);
            var args = BindGen.GetParamList(param);

            BindGen.Module.InvokeVoid("funcvoidawaitwin", funcname, args, errH);
            BindGen.ParamPool.Return(args);

            (object, string) tpl;
            while (!JCallBackHandler.ErrorMessages.TryGetValue(errH, out _))
            {
                await Task.Delay(5);
            }
            JCallBackHandler.ErrorMessages.TryRemove(errH, out tpl);
            if (!string.IsNullOrWhiteSpace(tpl.Item2))
                throw new Exception(tpl.Item2);
        }

        public async ValueTask<T> FuncAwaitAsync<T>(string funcname, params object[] param)
        {
            long errH = Interlocked.Increment(ref JCallBackHandler.ErrorTrack);
            var args = BindGen.GetParamList(param);
            BindGen.Module.InvokeVoid("funcawaitwin", funcname, args, errH);
            BindGen.ParamPool.Return(args);

            (object, string) tpl;
            while (!JCallBackHandler.ErrorMessages.TryGetValue(errH, out _))
            {
                await Task.Delay(5);
            }
            JCallBackHandler.ErrorMessages.TryRemove(errH, out tpl);
            if (!string.IsNullOrWhiteSpace(tpl.Item2))
                throw new Exception(tpl.Item2);
            if (tpl.Item1 is null) return default(T);
            var json = ((JsonElement)tpl.Item1).GetRawText();
            return JsonSerializer.Deserialize<T>(json);

        }

        public JObjPtr Construct(string classname, params object[] param)
        {
            JObjPtr ptr = new();
            var args = BindGen.GetParamList(param);
            BindGen.Module.InvokeVoid("constructwin",classname,args,ptr.Hash);
            BindGen.ParamPool.Return(args);
            return ptr;
        }
    }
}
