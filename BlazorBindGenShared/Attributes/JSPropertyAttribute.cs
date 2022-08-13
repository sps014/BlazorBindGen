using System;
using System.Collections.Generic;
using System.Text;
namespace BlazorBindGen.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class JSPropertyAttribute:Attribute
{
    public JSPropertyAttribute(string Name = null)
    {
    }
}