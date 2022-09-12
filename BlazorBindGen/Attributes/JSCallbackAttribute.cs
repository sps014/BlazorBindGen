using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorBindGen.Attributes;

[AttributeUsage(AttributeTargets.Delegate,AllowMultiple =false,Inherited =false)]
public class JSCallbackAttribute:Attribute
{
}
