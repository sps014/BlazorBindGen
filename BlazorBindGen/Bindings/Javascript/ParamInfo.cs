using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBindGen
{
    public class ParamInfo
    {
        public object Value { get; set; }
        public ParamTypes Type { get; set; } = ParamTypes.BASE;
    }
    public enum ParamTypes
    {
        BASE=0,
        JOBJ=1,
    }
}
