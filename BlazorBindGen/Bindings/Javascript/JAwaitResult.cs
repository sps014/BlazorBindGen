using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBindGen
{
    public class JAwaitResult<T>
    {
        public T Value { get; set; }
        public string Err { get; set; }
    }
}
