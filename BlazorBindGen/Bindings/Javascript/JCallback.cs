using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlazorBindGen
{
    public abstract class JCallback
    {
        public abstract void Execute(params object[] args);

        internal DotNetObjectReference<JCallback> DotNet;
        public JCallback()
        {
            DotNet = DotNetObjectReference.Create(this);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [JSInvokable("ExecuteInCSharp")]
        public void CallMe(object[] obj)
        {
            Execute(obj);
        }
    }
    
}
