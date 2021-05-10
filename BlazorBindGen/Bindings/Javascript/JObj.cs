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
    public class JObj : IJavaScriptObject
    {
        internal int Hash { get; set; }
        internal static int HashTrack = 0;
        

        internal JObj()
        {
            Hash = Interlocked.Increment(ref HashTrack);
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
        public void SetVal<T>(string propname, T value)
        {
            BindGen.Module.InvokeVoid("propset", propname, value,Hash);
        }

        public void SetPropRef(string propname, JObj obj)
        {
            BindGen.Module.InvokeVoid("propsetref", propname, obj.Hash,Hash);
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

        public async ValueTask<JObj> FuncRefAwaitAsync(string funcname, params object[] param)
        {

            JObj obj = new();
            long errH = Interlocked.Increment(ref JCallBackHandler.ErrorTrack);

            BindGen.Module.InvokeVoid("funcrefawait", funcname, BindGen.GetParamList(param), errH, obj.Hash,Hash);
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

            BindGen.Module.InvokeVoid("funcvoidawait", funcname, BindGen.GetParamList(param), errH,Hash);
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

            BindGen.Module.InvokeVoid("funcawait", funcname, BindGen.GetParamList(param), errH, Hash);
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
    }
}
