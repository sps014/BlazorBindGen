using Microsoft.JSInterop;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBindGen
{
    public static class BindGen
    {
        private static Lazy<Task<IJSInProcessObjectReference>> moduleTask;
        public static IJSInProcessObjectReference Module { get; private set; }
        public static JWindow Window { get; private set; }

        public static async ValueTask Init(IJSRuntime jsRuntime)
        {
            var runtime = jsRuntime as IJSInProcessRuntime;
            moduleTask = new(() => runtime.InvokeAsync<IJSInProcessObjectReference>(
               "import", "./_content/BlazorBindGen/blazorbindgen.js").AsTask());

            Window = JWindow.CreateJWindowObject();
            Module = await moduleTask.Value;
        }

        public static async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ParamInfo[] GetParamList(params object[] array)
        {
            var list=new List<ParamInfo>(array.Length);
            foreach (var p in array)
            {
                if(p is JObj)
                {
                    list.Add(new() { Value = (p as JObj).Hash, Type=ParamTypes.JOBJ});
                }
                else
                {
                    list.Add(new() { Value = p });
                }
            }
            return list.ToArray();
        }

    }
}
