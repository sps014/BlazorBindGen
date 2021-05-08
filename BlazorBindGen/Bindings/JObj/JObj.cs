using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorBindGen
{
    public class JObj : IJavaScriptObject
    {
        internal int Hash { get; set; }
        internal static int HashTrack = 0;

        internal JObj()
        {
            Hash = HashTrack++;
        }
        ~JObj()
        {
            BindGen.Module.InvokeVoid("deleteprop", Hash);
        }

        public T Val<T>(string propname)
        {
            return BindGen.Module.Invoke<T>("propval", propname, Hash);
        }
        public async ValueTask<T> ValAsync<T>(string propname)
        {
            return await BindGen.Module.InvokeAsync<T>("propval", propname, Hash);
        }
        public JObj PropRef(string propname)
        {
            var obj = new JObj();
            BindGen.Module.InvokeVoid("propref", propname, obj.Hash, Hash);
            return obj;
        }
        public async ValueTask<JObj> PropRefAsync(string propname)
        {
            var obj = new JObj();
            await BindGen.Module.InvokeVoidAsync("propref", propname, obj.Hash, Hash);
            return obj;
        }

        public bool IsProp(string propname)
        {
            return BindGen.Module.Invoke<bool>("isprop", propname, Hash);
        }
        public bool IsFunc(string propname)
        {
            return BindGen.Module.Invoke<bool>("isfunc", propname, Hash);
        }
    }
}
