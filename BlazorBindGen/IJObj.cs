using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorBindGen
{
    public interface IJObj
    {
        public T Prop<T>(string propname);
        public ValueTask<T> PropAsync<T>(string propname);
        public JObj PropRef(string propname);
        public ValueTask<JObj> PropRefAsync(string propname);
    }
}