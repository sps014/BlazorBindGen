using System.Runtime.InteropServices;

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
