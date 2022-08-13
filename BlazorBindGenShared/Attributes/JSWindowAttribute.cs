using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorBindGen.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct,AllowMultiple = false,Inherited =false)]
public class JSWindowAttribute:Attribute
{
    public JSWindowAttribute(string Name=null)
    {
    }
}
