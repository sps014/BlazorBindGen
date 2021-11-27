using Microsoft.JSInterop;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorBindGen
{
    public static class BindGen
    {
        public static IJSUnmarshalledObjectReference Module { get; private set; }
        public static IJSObjectReference GeneralizedModule { get; private set; }
        public static JWindow Window { get; private set; }

        private static DotNetObjectReference<JCallBackHandler> _dotNet;
        private static IJSInProcessRuntime Runtime { get; set; }
        private static IJSRuntime GeneralizedRuntime { get; set; }
        public static bool IsWasm { get; private set; } = false;
        public static async ValueTask InitAsync(IJSRuntime jsRuntime)
        {
            if(OperatingSystem.IsBrowser())
                IsWasm = true;

            if (IsWasm)
            {
                Runtime = jsRuntime as IJSInProcessRuntime;
                Module=await Runtime.InvokeAsync<IJSUnmarshalledObjectReference>(
                   "import", "./_content/BlazorBindGen/blazorbindgen.js");
            }
            else
            {
                GeneralizedModule = await jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/BlazorBindGen/blazorbindgen.js");

            }
            _dotNet = DotNetObjectReference.Create(new JCallBackHandler());
            Window = await JWindow.CreateJWindowObject();

            await InitDotNet();
        }
        private static async ValueTask InitDotNet()
        {
            if (IsWasm)
                Module.InvokeVoid("initDotnet", _dotNet);
            else
                await GeneralizedModule.InvokeVoidAsync("initDotnet", _dotNet);
        }

        public static async ValueTask DisposeAsync()
        {
            if (IsWasm)
                await Module.DisposeAsync();
            else
                await GeneralizedModule.DisposeAsync();
        }
        
        public static JObjPtr SetArrayToRef(byte[] array)
        {
            if(!IsWasm)
                throw new PlatformNotSupportedException("Only intended for wasm");

            var obj = new JObjPtr();
            _ = Module.InvokeUnmarshalled<byte[], int, object>("setarrayref", array, obj.Hash);
            return obj;
        }
        public static byte[] GetArrayFromRef(JObjPtr jsUint8ArrayRef)
        {
            if (!IsWasm)
                throw new PlatformNotSupportedException("Only intended for wasm");


            if (!jsUint8ArrayRef.IsProp("length"))
                throw new Exception("Invalid js array reference, make sure the pointer to  array from js should be correct.");
            var l = FastLength(jsUint8ArrayRef);
            var arr = new byte[l];
            _ = Module.InvokeUnmarshalled<byte[], int, object>("getarrayref", arr, jsUint8ArrayRef.Hash);
            return arr;
        }

        private static long FastLength(JObjPtr jsUint8ArrayRef) => 
            Module.InvokeUnmarshalled<int, int>("fastlength", jsUint8ArrayRef.Hash);
        public static async ValueTask ImportAsync(string moduleUrl)
        {
            long errH = Interlocked.Increment(ref JCallBackHandler.ErrorTrack);

            if(IsWasm)
                Module.InvokeUnmarshalled<string, int, object>("importmod", moduleUrl, (int)errH);
            else
                await GeneralizedModule.InvokeVoidAsync("importgen",moduleUrl, (int)errH);
            
            await LockHandler.HoldVoid(errH);
        }

    }
}
