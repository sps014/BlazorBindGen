using Microsoft.JSInterop;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorBindGen
{
    public static class BindGen
    {
        private static Lazy<Task<IJSInProcessObjectReference>> moduleTask;
        public static IJSInProcessObjectReference Module { get; private set; }
        public static JWindow Window { get; private set; }

        internal static DotNetObjectReference<JCallBackHandler> DotNet;

        internal static ArrayPool<ParamInfo> ParamPool = ArrayPool<ParamInfo>.Shared;
        internal static IJSInProcessRuntime Runtime { get; private set; }

        public static async ValueTask Init(IJSRuntime jsRuntime)
        {
            Runtime = jsRuntime as IJSInProcessRuntime;
            moduleTask = new(() => Runtime.InvokeAsync<IJSInProcessObjectReference>(
               "import", "./_content/BlazorBindGen/blazorbindgen.js").AsTask());

            Module = await moduleTask.Value;
            DotNet = DotNetObjectReference.Create(new JCallBackHandler());
            Window = JWindow.CreateJWindowObject();

            InitDotNet();
        }
        private static void InitDotNet()
        {
            Module.InvokeVoid("initDotnet", DotNet);
        }

        public static async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }
        public static async ValueTask Import(string moduleURL)
        {
            long errH = Interlocked.Increment(ref JCallBackHandler.ErrorTrack);

            Module.InvokeVoid("importmod", moduleURL,errH);

            (object, string) tpl;
            while (!JCallBackHandler.ErrorMessages.TryGetValue(errH, out _))
            {
                await Task.Delay(5);
            }
            JCallBackHandler.ErrorMessages.TryRemove(errH, out tpl);
            if (!string.IsNullOrWhiteSpace(tpl.Item2))
                throw new Exception(tpl.Item2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ParamInfo[] GetParamList(params object[] array)
        {
            var list=ParamPool.Rent(array.Length);
            int i = 0;
            foreach (var p in array)
            {
                if (p is JObjPtr)
                {
                    list[i]=new() { Value = (p as JObjPtr).Hash, Type = ParamTypes.JOBJ };
                }
                else if (p is JCallback)
                {
                    list[i] = new() { Type = ParamTypes.CALLBACK, Value = (p as JCallback).DotNet };
                }
                else
                {
                    list[i]=new() { Value = p };
                }
                i++;
            }
            return list; 
        }

    }
}
