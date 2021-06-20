﻿using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
        public void CallMe(int hash,int argLength)
        {
            var ptr=GetArgAsPtr(hash);
            var arr=new JObjPtr[argLength];

            for (int i = 0; i < argLength; i++)
            {
                arr[i] = ptr.PropRef($"{i}");
            }
            Executor.Invoke(arr);
        }
        private JObjPtr GetArgAsPtr(int hash)
        {
            JObjPtr ptrs =new();
            BindGen.Module.InvokeVoid("cleanupargs",hash,ptrs.Hash);
            return ptrs;
        }
    }
    
}
