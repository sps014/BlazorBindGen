using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;
using BlazorBindGen.Utils;
using System.Reflection;

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
        internal static IJSInProcessObjectReference WasmModule { get; private set; }
        
        /// <summary>
        /// Used in Server Context for synchronized Interops
        /// </summary>
        internal static IJSObjectReference ServerModule { get; private set; }

        internal static IJSObjectReference CommonModule => IsWasm ? WasmModule : ServerModule;
        
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
                WasmModule=await Runtime!.InvokeAsync<IJSInProcessObjectReference>(
                   "import", "./_content/BlazorBindGen/BlazorBindGen.js");
            }
            else
            {
                ServerModule = await jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/BlazorBindGen/BlazorBindGen.js");

            }
            //Create DotNet Object to handle callbacks from JS and Also create a window object

        }


        private static readonly Dictionary<Delegate, DotNetObjectReference<object>> _callbackReferences = new();


        public static void AddEventListener(this IJSInProcessObjectReference jSInProcessObjectReference,string eventName,Delegate callback)
        {
            var target = callback.Target;

            if(target == null)
                throw new ArgumentNullException("Please pass instance method instead of static method");

            if (!callback.Method.IsPublic || callback.Method.IsGenericMethod)
                throw new ArgumentException("Callback method should be public and non generic");

            var jSInvokable = callback.Method.GetCustomAttribute<JSInvokableAttribute>();

            if (jSInvokable is null)
                throw new ArgumentNullException("[JSInvocableAttribute] Not found on callback method");

            var methodName = jSInvokable.Identifier ?? callback.Method.Name.Split(".").Last();

            var dotnetObjectReference = DotNetObjectReference.Create(callback.Target);

            try
            {
                WasmModule.InvokeVoid("addEventListenerCSharp", jSInProcessObjectReference, eventName,methodName,dotnetObjectReference);
                _callbackReferences.Add(callback, dotnetObjectReference);
            }
            catch(Exception)
            {
                dotnetObjectReference.Dispose();
            }
        }

        public static void RemoveEventListener(this IJSInProcessObjectReference jSInProcessObjectReference, string eventName, Delegate callback)
        {
            WasmModule.InvokeVoid("removeEventListenerCSharp", jSInProcessObjectReference, eventName);

            if (_callbackReferences.TryGetValue(callback,out DotNetObjectReference<object> dotnetRuntime))
            {
                dotnetRuntime.Dispose();
                _callbackReferences.Remove(callback);
            }
        }



    }
}
