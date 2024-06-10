namespace SampleApp.JSBinding;

[JSModule(nameof(FirebaseBinding))]
public partial class FirebaseModule
{
    [ModuleFunction("testFunc")]
    public partial void Method();
}

