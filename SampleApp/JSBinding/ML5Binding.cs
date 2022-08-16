using BlazorBindGen.Attributes;

namespace SampleApp.JSBinding
{
    [JSWindow]
    public static partial class DomWindow
    {
        [JSProperty]
        private static ML5 ml5;

    }
    [JSObject]
    public partial class ML5
    {
        [JSFunction("sentiment")]
        public partial Sentiment Sentiment(string modelName,OnModelLoadHandler onModelLoad);

        [JSCallback]
        public delegate void OnModelLoadHandler();
    }
    [JSObject]
    public partial class Sentiment
    {
        [JSFunction("predict")]
        public partial Score Predict(string text);
    }
    public record Score(double score);


}
