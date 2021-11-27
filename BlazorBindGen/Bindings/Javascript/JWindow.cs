using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorBindGen
{
    public sealed class JWindow : JObjPtr
    {

        private JWindow() { }

        internal static async ValueTask<JWindow> CreateJWindowObject()
        {
            JWindow win = new();
            if (BindGen.IsWasm)
                BindGen.Module.InvokeUnmarshalled<int, object>("createwin", win.Hash);
            else
                await BindGen.GeneralizedModule.InvokeVoidAsync("createwin", win.Hash);
            return win;
        }

    }
}
