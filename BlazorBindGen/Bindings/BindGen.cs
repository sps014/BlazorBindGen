using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
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

    }
}
