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
        private static Lazy<Task<IJSUnmarshalledObjectReference>> moduleTask;
        public static IJSUnmarshalledObjectReference Module { get; private set; }
        public static JWindow Window { get; private set; }

        internal static DotNetObjectReference<JCallBackHandler> DotNet;

        internal static ArrayPool<ParamInfo> ParamPool = ArrayPool<ParamInfo>.Shared;
        internal static IJSInProcessRuntime Runtime { get; private set; }
        public static async ValueTask Init(IJSRuntime jsRuntime)
        {
            Runtime = jsRuntime as IJSInProcessRuntime;
            moduleTask = new(() => Runtime.InvokeAsync<IJSUnmarshalledObjectReference>(
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
        public static JObjPtr SetArrayToRef(byte[] array)
        {
            var obj = new JObjPtr();
            _ = BindGen.Module.InvokeUnmarshalled<byte[], int, object>("setarrayref", array, obj.Hash);
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
        internal static long FastLength(JObjPtr jsUint8ArrayRef)
        {
            return Module.InvokeUnmarshalled<int, int>("fastlength", jsUint8ArrayRef.Hash);
        }
        public static async ValueTask Import(string moduleURL)
        {
            long errH = Interlocked.Increment(ref JCallBackHandler.ErrorTrack);

            Module.InvokeUnmarshalled<string, int, object>("importmod", moduleURL, (int)errH);

            await ErrorHandler.HoldVoid(errH);
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
                else if (p is Action<JObjPtr[]>)
                {
                    list[i] = new() { Type = ParamTypes.CALLBACK, Value = (new JCallback(p as Action<JObjPtr[]>)).DotNet };
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
