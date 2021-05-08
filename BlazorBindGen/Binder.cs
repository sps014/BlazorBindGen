using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBindGen
{
    public class Binder:IAsyncDisposable
    {
        private readonly Lazy<Task<IJSInProcessObjectReference>> moduleTask;
        public static IJSInProcessObjectReference Module { get; private set; }
        public JWindow ThisRef()
        {
            return new JWindow();
        }
        public  Binder(IJSRuntime jsRuntime)
        {
            var runtime = jsRuntime as IJSInProcessRuntime;
            moduleTask = new(() => runtime.InvokeAsync<IJSInProcessObjectReference>(
               "import", "./_content/BlazorBindGen/blazorbindgen.js").AsTask());
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
