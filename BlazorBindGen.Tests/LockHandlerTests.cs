using System.Text.Json;
using BlazorBindGen.Bindings;
using Xunit;

namespace BlazorBindGen.Tests;

/// <summary>
/// Platform-agnostic tests for the C#/JS async synchronization in <see cref="LockHandler"/>.
/// A <see cref="JCallBackHandler"/> instance is used to simulate the "errorMessage" callback that
/// the JS module normally invokes when an awaited JS operation completes.
/// </summary>
public class LockHandlerTests
{
    private static JsonElement Json(string raw) => JsonDocument.Parse(raw).RootElement.Clone();

    private static void RaiseJsCallback(long errH, string? error, object? value)
    {
        // Mirrors what BlazorBindGen.js does via dotnet.invokeMethodAsync("errorMessage", ...).
#pragma warning disable CS0618 // intentionally exercising the internal interop entry point
        new JCallBackHandler().ErrorMessageCallback(errH, error, value);
#pragma warning restore CS0618
    }

    [Fact]
    public async Task HoldVoid_CompletesWhenCallbackArrives()
    {
        const long errH = 1_000_001;

        // register the wait first (matches the production call sites)
        var hold = LockHandler.HoldVoid(errH);
        RaiseJsCallback(errH, error: null, value: null);

        await hold; // should complete without throwing
    }

    [Fact]
    public async Task HoldVoid_ThrowsWhenCallbackReportsError()
    {
        const long errH = 1_000_002;

        var hold = LockHandler.HoldVoid(errH);
        RaiseJsCallback(errH, error: "boom", value: null);

        var ex = await Assert.ThrowsAsync<Exception>(async () => await hold);
        Assert.Equal("boom", ex.Message);
    }

    [Fact]
    public async Task Hold_DeserializesReturnedValue()
    {
        const long errH = 1_000_003;

        var hold = LockHandler.Hold<int>(errH);
        RaiseJsCallback(errH, error: null, value: Json("42"));

        var result = await hold;
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task Hold_ReturnsDefaultForNullValue()
    {
        const long errH = 1_000_004;

        var hold = LockHandler.Hold<string>(errH);
        RaiseJsCallback(errH, error: null, value: null);

        var result = await hold;
        Assert.Null(result);
    }

    [Fact]
    public async Task Hold_ThrowsWhenCallbackReportsError()
    {
        const long errH = 1_000_005;

        var hold = LockHandler.Hold<int>(errH);
        RaiseJsCallback(errH, error: "explode", value: null);

        var ex = await Assert.ThrowsAsync<Exception>(async () => await hold);
        Assert.Equal("explode", ex.Message);
    }
}
