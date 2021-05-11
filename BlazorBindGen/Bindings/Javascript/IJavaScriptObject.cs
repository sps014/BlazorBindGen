using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace BlazorBindGen
{
    public interface IJavaScriptObject
    {
        public T Val<T>(string propname);
        public ValueTask<T> ValAsync<T>(string propname);
        public JObjPtr PropRef(string propname);
        public ValueTask<JObjPtr> PropRefAsync(string propname);
        public void SetVal<T>(string propname,T value);
        public void SetPropRef(string propname,JObjPtr obj);
        public bool IsProp(string propname);
        public bool IsFunc(string propname);
        public T Func<T>(string funcname, params object[] param);
        public void FuncVoid(string funcname, params object[] param);
        public void FuncVoidAsync(string funcname, params object[] param);

        public ValueTask<T> FuncAsync<T>(string funcname,params object[] param);
        public JObjPtr FuncRef(string funcname, params object[] param);
        public ValueTask<JObjPtr> FuncRefAsync(string funcname, params object[] param);

        public ValueTask FuncVoidAwaitAsync(string funcname, params object[] param);
        public ValueTask<T> FuncAwaitAsync<T>(string funcname, params object[] param);
        public ValueTask<JObjPtr> FuncRefAwaitAsync(string funcname, params object[] param);
        public JObjPtr Construct(string classname, params object[] param);
    }
}