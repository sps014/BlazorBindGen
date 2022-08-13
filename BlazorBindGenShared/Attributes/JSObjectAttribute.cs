using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorBindGen.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public class JSObjectAttribute:Attribute
{
    public JSObjectAttribute(string Name = null,string importUrl=null)
    {
    }
}
