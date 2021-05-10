using Microsoft.JSInterop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBindGen
{
    public class JCallBackHandler
    {
        internal static ConcurrentDictionary<long,(object Value,string Error)> ErrorMessages = new();
        internal static long ErrorTrack = 0;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [JSInvokable("errorMessage")]
        public void ErrorMessageCallback(long ec, string error,object v)
        {
            ErrorMessages.TryAdd(ec, (v,error));
        }

    }
}
