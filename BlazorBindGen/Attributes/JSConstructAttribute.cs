﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorBindGen.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class JSConstructAttribute : Attribute
{
    public JSConstructAttribute(string Name = null)
    {
    }
}
