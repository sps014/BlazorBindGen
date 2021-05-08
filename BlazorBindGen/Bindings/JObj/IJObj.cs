﻿using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorBindGen
{
    public interface IJObj
    {
        public T Val<T>(string propname);
        public ValueTask<T> ValAsync<T>(string propname);
        public JObj PropRef(string propname);
        public ValueTask<JObj> PropRefAsync(string propname);
    }
}