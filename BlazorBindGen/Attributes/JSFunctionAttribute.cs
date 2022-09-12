using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorBindGen.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class JSFunctionAttribute : Attribute
{
    public JSFunctionAttribute(string Name = null)
    {
    }
}
