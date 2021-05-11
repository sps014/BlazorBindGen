using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBindGen
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ParamInfo
    {
        public object Value { get; set; }
        public ParamTypes Type { get; set; }
    }
    public enum ParamTypes
    {
        BASE=0,
        JOBJ=1,
        CALLBACK=2,
    }
}
