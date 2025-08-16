using BlazorBindGen.Attributes;

namespace SampleApp.JSBinding
{
    [JSWindow]
    public static partial class DomWindow
    {
        [JSProperty]
        private static ML5 ml5;

    }
    [JSObject("https://unpkg.com/ml5@1/dist/ml5.js")]
    public partial class ML5
    {
        [JSFunction("sentiment")]
        public partial Sentiment Sentiment(string modelName,OnModelLoadHandler onModelLoad);

        [JSCallback]
        public delegate void OnModelLoadHandler();

        [JSConstruct("p5")]
        public partial Sentiment Construct();
    }
    [JSObject]
    public partial class Sentiment
    {
        [JSFunction("predict")]
        public partial void Predict(string text, GotResultHandler gotResult);

        [JSCallback]
        public delegate void GotResultHandler(Score score);
    }
    public record Score(double confidence);


}
