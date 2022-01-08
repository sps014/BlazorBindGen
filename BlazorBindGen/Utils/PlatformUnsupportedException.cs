using System.Runtime.CompilerServices;

namespace BlazorBindGen.Utils;

public class PlatformUnsupportedException:Exception
{
    public PlatformUnsupportedException(string error) :base(error)
    {
    }
    public static PlatformUnsupportedException Throw([CallerMemberName] string caller = "")
    {
        throw new PlatformUnsupportedException($"Function `{caller}` is only supported on WASM platform, see if Async Version of '{caller}' is available");
    }
}