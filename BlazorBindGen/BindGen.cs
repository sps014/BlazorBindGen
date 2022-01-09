using BlazorBindGen.Bindings;
using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;
using BlazorBindGen.Utils;

namespace BlazorBindGen
{
    /// <summary>
    /// Class Responsible for generating the bindings for a given type.
    /// </summary>
    public static class BindGen
    {
        
        #nullable disable
        /// <summary>
        /// Used in only Web Assembly Context for faster Interops
        /// </summary>
        public static IJSUnmarshalledObjectReference Module { get; private set; }
        
        /// <summary>
        /// Used in Server Context for synchronized Interops
        /// </summary>
        public static IJSObjectReference GeneralizedModule { get; private set; }
        
        /// <summary>
        /// get Current JS Window.
        /// eg. window.document.getElementById("id") here Window is the current window.
        /// </summary>
        public static JWindow Window { get; private set; }
        
        /// <summary>
        /// Dotnet Object to handle callbacks from JS
        /// </summary>
        private static DotNetObjectReference<JCallBackHandler> _dotNet;
        /// <summary>
        /// Used to load JS isolated modules in WASM
        /// </summary>
        private static IJSInProcessRuntime Runtime { get; set; }
 
        /// <summary>
        /// Check whether the current platform is WebAssembly or not (works only in Runtime)
        /// </summary>
        public static bool IsWasm { get; private set; }
        
        /// <summary>
        /// Initialize the BindGen Library for JS Interops from C#
        /// </summary>
        /// <param name="jsRuntime">JS Runtime object (use `@inject IJSRuntime Runtime` to get one in razor</param>
        public static async ValueTask InitAsync([NotNull]IJSRuntime jsRuntime)
        {
            if(OperatingSystem.IsBrowser())
                IsWasm = true;
            
            //Load JS isolated module in WASM and Server Side
            if (IsWasm)
            {
                Runtime = jsRuntime as IJSInProcessRuntime;
                Module=await Runtime!.InvokeAsync<IJSUnmarshalledObjectReference>(
                   "import", "./_content/BlazorBindGen/BlazorBindGen.js");
            }
            else
            {
                GeneralizedModule = await jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/BlazorBindGen/BlazorBindGen.js");

            }
            //Create DotNet Object to handle callbacks from JS and Also create a window object
            _dotNet = DotNetObjectReference.Create(new JCallBackHandler());
            Window = await JWindow.CreateJWindowObject();

            await InitDotNet();
        }
        /// <summary>
        /// Pass .NET object to JS so it can call C# for Callbacks notification
        /// </summary>
        private static async ValueTask InitDotNet()
        {
            if (IsWasm)
                Module.InvokeVoid("initDotnet", _dotNet);
            else
                await GeneralizedModule.InvokeVoidAsync("initDotnet", _dotNet);
        }
#nullable restore
        
        /// <summary>
        /// Dispose the BindGen Runtimes
        /// </summary>
        public static async ValueTask DisposeAsync()
        {
            if (IsWasm)
                await Module.DisposeAsync();
            else
                await GeneralizedModule.DisposeAsync();
        }
        
        /// <summary>
        /// A Fast Method to get JS pointer to byte Array in WASM platform
        /// </summary>
        /// <param name="array">Array from C# to sent to JS</param>
        /// <returns>JS pointer</returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public static JObjPtr SetArrayToRef(byte[] array)
        {
            if (!IsWasm)
                PlatformUnsupportedException.Throw();

            var obj = new JObjPtr();
            _ = Module.InvokeUnmarshalled<byte[], int, object>("SetArrayToRef", array, obj.Hash);
            return obj;
        }
        /// <summary>
        /// Fastest way to Get Byte Array from JS pointer in WASM platform
        /// </summary>
        /// <param name="jsUint8ArrayRef"> js pointer uint8 array </param>
        /// <returns>byte array representation</returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        /// <exception cref="Exception"></exception>
        public static byte[] GetArrayFromRef(JObjPtr jsUint8ArrayRef)
        {
            if (!IsWasm)
                throw new PlatformNotSupportedException("Only intended for wasm");
            //if has length property then it is a JS Array
            if (!jsUint8ArrayRef.IsProp("length"))
                throw new Exception("Invalid js array reference, make sure the pointer to  array from js should be correct.");
            
            //Get length of array
            var l = FastLength(jsUint8ArrayRef);
            var arr = new byte[l];
            
            //Get array from js
            _ = Module.InvokeUnmarshalled<byte[], int, object>("GetArrayRef", arr, jsUint8ArrayRef.Hash);
            return arr;
        }
        /// <summary>
        /// Get Length of Uint8 array JS in WASM
        /// </summary>
        /// <param name="jsUint8ArrayRef"> pointer to Unit8Array</param>
        /// <returns>size of array</returns>
        private static long FastLength(JObjPtr jsUint8ArrayRef) => 
            Module.InvokeUnmarshalled<long, int>("FastLength", jsUint8ArrayRef.Hash);
        
        /// <summary>
        /// Import external or internal JS module, equivalent to import in JS
        /// </summary>
        /// <param name="moduleUrl">uri of the module</param>
        public static async ValueTask ImportAsync(string moduleUrl)
        {
            //Increment Sync callback id
            long errH = Interlocked.Increment(ref JCallBackHandler.SyncCounter);

            if(IsWasm)
                Module.InvokeUnmarshalled<string, int, object>("ImportWasm", moduleUrl, (int)errH);
            else
                await GeneralizedModule.InvokeVoidAsync("ImportGen",moduleUrl, (int)errH);
            
            //wait until both runtimes are synced
            await LockHandler.HoldVoid(errH);
        }

    }
}
