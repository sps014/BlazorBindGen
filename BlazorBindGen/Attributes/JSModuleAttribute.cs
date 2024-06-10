using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBindGen.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false,Inherited = false)]
public class JSModuleAttribute:Attribute
{
    public JSModuleAttribute(string moduleUrl)
    {

    }
}
