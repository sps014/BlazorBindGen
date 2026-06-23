using BlazorBindGen;
using BlazorBindGen.Javascript;
using Xunit;

namespace BlazorBindGen.Tests;

/// <summary>
/// Platform-agnostic tests for <see cref="JObjPtr.GetParamList"/> parameter tagging.
/// These do not touch the browser/JS runtime.
/// </summary>
public class ParamListTests
{
    [Fact]
    public void EmptyParams_ReturnSharedEmptyArray()
    {
        var result = JObjPtr.GetParamList();

        Assert.Empty(result);
        Assert.Same(Array.Empty<ParamInfo>(), result);
    }

    [Fact]
    public void JObjPtrParam_IsTaggedAsJObjPtrWithHashValue()
    {
        var ptr = new JObjPtr();

        var result = JObjPtr.GetParamList(ptr);

        Assert.Single(result);
        Assert.Equal(ParamTypes.JObjPtr, result[0].Type);
        Assert.Equal(ptr.Hash, result[0].Value);
    }

    [Fact]
    public void CallbackParam_IsTaggedAsCallback()
    {
        Action<JObjPtr[]> callback = _ => { };

        var result = JObjPtr.GetParamList(callback);

        Assert.Single(result);
        Assert.Equal(ParamTypes.Callback, result[0].Type);
        Assert.NotNull(result[0].Value);
    }

    [Fact]
    public void PlainParams_AreTaggedAsDefaultAndPassedThrough()
    {
        var result = JObjPtr.GetParamList(42, "hello");

        Assert.Equal(2, result.Length);

        Assert.Equal(ParamTypes.Default, result[0].Type);
        Assert.Equal(42, result[0].Value);

        Assert.Equal(ParamTypes.Default, result[1].Type);
        Assert.Equal("hello", result[1].Value);
    }

    [Fact]
    public void MixedParams_PreserveOrderAndTags()
    {
        var ptr = new JObjPtr();
        Action<JObjPtr[]> callback = _ => { };

        var result = JObjPtr.GetParamList(ptr, "x", callback);

        Assert.Equal(3, result.Length);
        Assert.Equal(ParamTypes.JObjPtr, result[0].Type);
        Assert.Equal(ParamTypes.Default, result[1].Type);
        Assert.Equal(ParamTypes.Callback, result[2].Type);
    }
}
