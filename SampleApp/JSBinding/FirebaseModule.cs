namespace SampleApp.JSBinding;

[JSModule(nameof(FirebaseBinding))]
public partial class FirebaseModule
{
    [ModuleFunction("testFunc")]
    public partial void Method();

    [ModuleFunction("testFunc2")]
    public partial Task Method2();
    [ModuleFunction("testFunc3")]
    public partial int Method3();
}

