using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace BlazorBindGen
{
    public interface IJavaScriptObject
    {
        public T PropVal<T>(string propname);
        public ValueTask<T> PropValAsync<T>(string propname);
        public JObjPtr PropRef(string propname);
        public ValueTask<JObjPtr> PropRefAsync(string propname);
        public void SetPropVal<T>(string propname,T value);
        public void SetPropRef(string propname,JObjPtr obj);
        public void SetPropCallBack(string propname, Action<object[]> callback);
        public bool IsProp(string propname);
        public bool IsFunc(string propname);
        public T Call<T>(string funcname, params object[] param);
        public void CallVoid(string funcname, params object[] param);
        public void CallVoidAsync(string funcname, params object[] param);

        public ValueTask<T> CallAsync<T>(string funcname,params object[] param);
        public JObjPtr CallRef(string funcname, params object[] param);
        public ValueTask<JObjPtr> CallRefAsync(string funcname, params object[] param);

        public ValueTask CallVoidAwaitedAsync(string funcname, params object[] param);
        public ValueTask<T> CallAwaitedAsync<T>(string funcname, params object[] param);
        public ValueTask<JObjPtr> CallRefAwaitedAsync(string funcname, params object[] param);
        public JObjPtr Construct(string classname, params object[] param);
    }
}