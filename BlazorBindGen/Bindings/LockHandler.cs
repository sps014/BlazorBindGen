using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorBindGen
{
    public static class LockHandler
    {
        public static async ValueTask HoldVoid(long errH)
        {
            while (!JCallBackHandler.ErrorMessages.TryGetValue(errH, out _))
            {
                await Task.Delay(5);
            }
            _ = JCallBackHandler.ErrorMessages.TryRemove(errH, out (object, string) tpl);
            if (!string.IsNullOrWhiteSpace(tpl.Item2))
                throw new Exception(tpl.Item2);
        }
        public static async ValueTask<T> Hold<T>(long errH)
        {
            while (!JCallBackHandler.ErrorMessages.TryGetValue(errH, out _))
            {
                await Task.Delay(5);
            }
            _ = JCallBackHandler.ErrorMessages.TryRemove(errH, out (object, string) tpl);
            if (!string.IsNullOrWhiteSpace(tpl.Item2))
                throw new Exception(tpl.Item2);
            if (tpl.Item1 is null) return default(T);
            var json = ((JsonElement)tpl.Item1).GetRawText();
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
