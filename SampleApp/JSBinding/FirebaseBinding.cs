namespace SampleApp.JSBinding;

[JSObject("https://www.gstatic.com/firebasejs/9.8.2/firebase-app.js")]
public partial class FirebaseBinding
{
    [JSProperty("SDK_VERSION",true,false)]
    private string sdkVersion;
}
