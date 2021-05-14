using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlazorBindGen
{
    internal class JCallback
    {
        internal DotNetObjectReference<JCallback> DotNet;

        public Action<object[]> Executor { get; }
        public JCallback([NotNull] Action<object[]> action)
        {
            DotNet = DotNetObjectReference.Create(this);
            Executor = action;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [JSInvokable("ExecuteInCSharp")]
        public void CallMe(object[] obj)
        {
            Console.WriteLine("Loaded");
            //Executor.Invoke(obj);
        }
    }
    
}
