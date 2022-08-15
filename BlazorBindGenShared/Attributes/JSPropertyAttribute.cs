using System;
using System.Collections.Generic;
using System.Text;
namespace BlazorBindGen.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
public class JSPropertyAttribute:Attribute
{
    public JSPropertyAttribute(string Name,bool generateGetter=true,bool generateSetter=true)
    {
    }
    public JSPropertyAttribute(bool generateGetter=true, bool generateSetter = true)
    {
    }
}
