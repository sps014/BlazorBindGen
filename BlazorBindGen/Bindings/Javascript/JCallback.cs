using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.JSInterop;
namespace BlazorBindGen
{
    public class JCallback
    {
        internal readonly DotNetObjectReference<JCallback> DotNet;

        public Action<JObjPtr[]> Executor { get; }
        public JCallback([NotNull] Action<JObjPtr[]> action)
        {
            DotNet = DotNetObjectReference.Create(this);
            Executor = action;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [JSInvokable("ExecuteInCSharp")]
        public async Task CallMe(int hash,int argLength)
        {
            var ptr=await GetArgAsPtrAsync(hash);
            var arr=new JObjPtr[argLength];

            for (int i = 0; i < argLength; i++)
            {
                arr[i] = ptr.PropRef($"{i}");
            }
            Executor.Invoke(arr);
        }
        private async ValueTask<JObjPtr> GetArgAsPtrAsync(int hash)
        {
            JObjPtr ptrs =new();
            if(BindGen.IsWasm)
                BindGen.Module.InvokeVoid("cleanupargs",hash,ptrs.Hash);
            else
                await BindGen.GeneralizedModule.InvokeVoidAsync("cleanupargs", hash, ptrs.Hash);
            return ptrs;
        }
    }
    
}
