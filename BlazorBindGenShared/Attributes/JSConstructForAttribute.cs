using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorBindGen.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class JSConstructForAttribute<T> : Attribute
{
    public JSConstructForAttribute(string Name = null)
    {
    }
}
