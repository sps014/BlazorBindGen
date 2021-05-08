using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBindGen
{
    public class BindGen:IAsyncDisposable
    {
        private readonly Lazy<Task<IJSInProcessObjectReference>> moduleTask;
        public static IJSInProcessObjectReference Module { get; private set; }
        public static JWindow This { get; private set; }
        public  BindGen(IJSRuntime jsRuntime)
        {
            var runtime = jsRuntime as IJSInProcessRuntime;
            moduleTask = new(() => runtime.InvokeAsync<IJSInProcessObjectReference>(
               "import", "./_content/BlazorBindGen/blazorbindgen.js").AsTask());

            This = JWindow.CreateJWindowObject();
        }
        public async ValueTask Init()
        {

            Module = await moduleTask.Value;
        }
       

        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }

    }
}
