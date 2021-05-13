using Microsoft.JSInterop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorBindGen
{
    public class JObjPtr : IJavaScriptObject
    {
        internal int Hash { get; set; }
        internal static int HashTrack = 0;
        

        internal JObjPtr()
        {
            Hash = Interlocked.Increment(ref HashTrack);
        }
        ~JObjPtr()
        {
            BindGen.Module.InvokeUnmarshalled<int,object>("deleteprop", Hash);
        }

        
        public T Val<T>(string propname)
        {
            return BindGen.Module.Invoke<T>("propval", propname, Hash);
        }
        public async ValueTask<T> ValAsync<T>(string propname)
        {
            return await BindGen.Module.InvokeAsync<T>("propval", propname, Hash);
        }
        public JObjPtr PropRef(string propname)
        {
            var obj = new JObjPtr();
            BindGen.Module.InvokeUnmarshalled<string,int,int,object>("propref", propname, obj.Hash, Hash);
            return obj;
        }
        public async ValueTask<JObjPtr> PropRefAsync(string propname)
        {
            var obj = new JObjPtr();
            await BindGen.Module.InvokeVoidAsync("propref", propname, obj.Hash, Hash);
            return obj;
        }
        public void SetVal<T>(string propname, T value)
        {
            BindGen.Module.InvokeVoid("propset", propname, value,Hash);
        }

        public void SetPropRef(string propname, JObjPtr obj)
        {
            BindGen.Module.InvokeVoid("propsetref", propname, obj.Hash,Hash);
        }

        public bool IsProp(string propname)
        {
            return BindGen.Module.InvokeUnmarshalled<string,int,bool>("isprop", propname, Hash);
        }
        public bool IsFunc(string propname)
        {
            return BindGen.Module.Invoke<bool>("isfunc", propname, Hash);
        }

        public T Func<T>(string funcname, params object[] param)
        {
            var args = BindGen.GetParamList(param);
            var res= BindGen.Module.Invoke<T>("func", funcname, args, Hash);
            BindGen.ParamPool.Return(args);
            return res;
        }

        public async ValueTask<T> FuncAsync<T>(string funcname, params object[] param)
        {
            var args = BindGen.GetParamList(param);
            var res= await BindGen.Module.InvokeAsync<T>("func", funcname, args, Hash);
            BindGen.ParamPool.Return(args);
            return res;
        }

        public JObjPtr FuncRef(string funcname, params object[] param)
        {
            var args = BindGen.GetParamList(param);
            JObjPtr j = new();
            BindGen.Module.InvokeVoid("funcref", funcname,args, j.Hash,Hash);
            BindGen.ParamPool.Return(args);
            return j;
        }

        public async ValueTask<JObjPtr> FuncRefAsync(string funcname, params object[] param)
        {
            JObjPtr j = new();
            var args = BindGen.GetParamList(param);
            await BindGen.Module.InvokeVoidAsync("funcref", funcname, args, j.Hash,Hash);
            BindGen.ParamPool.Return(args);
            return j;
        }
        public void FuncVoid(string funcname, params object[] param)
        {
            var args = BindGen.GetParamList(param);
            BindGen.Module.InvokeVoid("funcvoid", funcname, args, Hash);
            BindGen.ParamPool.Return(args);
        }

        public async void FuncVoidAsync(string funcname, params object[] param)
        {
            var args = BindGen.GetParamList(param);
            await BindGen.Module.InvokeVoidAsync("funcvoid", funcname, args, Hash);
            BindGen.ParamPool.Return(args);
        }

        public async ValueTask<JObjPtr> FuncRefAwaitAsync(string funcname, params object[] param)
        {

            JObjPtr obj = new();
            long errH = Interlocked.Increment(ref JCallBackHandler.ErrorTrack);
            var args = BindGen.GetParamList(param);

            BindGen.Module.InvokeVoid("funcrefawait", funcname, args, errH, obj.Hash,Hash);
            BindGen.ParamPool.Return(args);

            (object, string) tpl;
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
            BindGen.Module.InvokeVoid("funcvoidawait", funcname, args, errH,Hash);
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

            BindGen.Module.InvokeVoid("funcawait", funcname, args, errH, Hash);
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

        public string AsJsonText()
        {
            return BindGen.Module.Invoke<string>("asjson",Hash);
        }
        public T To<T>()
        {
            return BindGen.Module.Invoke<T>("to", Hash);
        }

        public JObjPtr Construct(string classname, params object[] param)
        {
            JObjPtr ptr = new();
            var args = BindGen.GetParamList(param);
            BindGen.Module.InvokeVoid("construct", classname, args,ptr.Hash,Hash);
            BindGen.ParamPool.Return(args);
            return ptr;
        }
    }
}
