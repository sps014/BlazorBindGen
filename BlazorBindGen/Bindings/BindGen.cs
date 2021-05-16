using Microsoft.JSInterop;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorBindGen
{
    public static class BindGen
    {
        private static Lazy<Task<IJSUnmarshalledObjectReference>> _moduleTask;
        public static IJSUnmarshalledObjectReference Module { get; private set; }
        public static JWindow Window { get; private set; }

        private static DotNetObjectReference<JCallBackHandler> _dotNet;
        private static IJSInProcessRuntime Runtime { get; set; }
        public static async ValueTask Init(IJSRuntime jsRuntime)
        {
            Runtime = jsRuntime as IJSInProcessRuntime;
            _moduleTask = new(() => Runtime.InvokeAsync<IJSUnmarshalledObjectReference>(
               "import", "./_content/BlazorBindGen/blazorbindgen.js").AsTask());
            Module = await _moduleTask.Value;

            _dotNet = DotNetObjectReference.Create(new JCallBackHandler());
            Window = JWindow.CreateJWindowObject();

            InitDotNet();
        }
        private static void InitDotNet()
        {
            Module.InvokeVoid("initDotnet", _dotNet);
        }

        public static async ValueTask DisposeAsync()
        {
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value;
                await module.DisposeAsync();
            }
        }
        public static JObjPtr SetArrayToRef(byte[] array)
        {
            var obj = new JObjPtr();
            _ = Module.InvokeUnmarshalled<byte[], int, object>("setarrayref", array, obj.Hash);
            return obj;
        }
        public static byte[] GetArrayFromRef(JObjPtr jsUint8ArrayRef)
        {
            if (!jsUint8ArrayRef.IsProp("length"))
                throw new Exception("Invalid js array reference, make sure the pointer to  array from js should be correct.");
            var l = FastLength(jsUint8ArrayRef);
            var arr = new byte[l];
            _ = Module.InvokeUnmarshalled<byte[], int, object>("getarrayref", arr, jsUint8ArrayRef.Hash);
            return arr;
        }

        private static long FastLength(JObjPtr jsUint8ArrayRef) => 
            Module.InvokeUnmarshalled<int, int>("fastlength", jsUint8ArrayRef.Hash);
        public static async ValueTask Import(string moduleURL)
        {
            long errH = Interlocked.Increment(ref JCallBackHandler.ErrorTrack);

            Module.InvokeUnmarshalled<string, int, object>("importmod", moduleURL, (int)errH);

            await LockHandler.HoldVoid(errH);
        }

       
        
    }
}
