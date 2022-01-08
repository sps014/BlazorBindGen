using System.Runtime.InteropServices;

namespace BlazorBindGen.Javascript;

/// <summary>
/// It is used for sending Value and Type of Value to Javascript for parameters where parameters can be anything from ptr to callback methods
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct ParamInfo
{
    /// <summary>
    /// Actual value of the parameter
    /// </summary>
    public object? Value { get; set; }
    /// <summary>
    /// Type of the value passed as Parameter
    /// </summary>
    public ParamTypes Type { get; set; }
}

/// <summary>
/// Kind of Parameter passed
/// </summary>
internal enum ParamTypes
{
    /// <summary>
    /// Any type of parameter passed to like int , string class etc
    /// </summary>
    Default = 0,
    
    /// <summary>
    /// it is pointer to another JS object
    /// </summary>
    JObjPtr = 1,
    
    /// <summary>
    /// It is a callback method
    /// </summary>
    Callback = 2,
}
