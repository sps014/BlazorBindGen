using System;
using Microsoft.JSInterop;
using BlazorBindGen;
using System.Threading.Tasks;
using BlazorBindGen.Attributes;

namespace SampleApp;

[JSWindow]
public partial class Win
{
    [JSConstruct("audio")]
    private partial Audio Audio(string fileName);
    [JSConstruct]
    private partial Audio Audio();

}

[JSObject]
public partial class Audio
{


    //js property to c#
    [JSProperty(generateGetter:true,generateSetter:false)]
    private double duration;
    //map js audio currentTime to c# property
    [JSProperty]
    private double currentTime;
    //map js audio readyState property enum result to c#
    [JSProperty]
    private int readyState;

    public ReadyStates GetReadyState => (ReadyStates)ReadyState;


    [JSProperty(true,false)]
    private bool paused;

    [JSProperty]
    private double volume;

    [JSProperty]
    private bool muted;

    //call js setsource on _audio in js
    [JSFunction("setSource")]
    public partial void SetSource(string url);
    //call js pause() on _audio object
    [JSFunction("pause")]
    public partial void Pause();

    //call js play() on _audio object ; js side equivalent await _audio.play(); 
    [JSFunction("play")]
    public partial void Play();

    //onmetadatacallback 
    [JSProperty("onloadedmetadata")]
    [JSCallback]
    public delegate void OnLoadedMetaDataHandler();

    [JSProperty("oncanplay")]
    [JSCallback]
    public delegate void OnCanPlayHandler();

    [JSProperty("ontimeupdate")]
    [JSCallback]
    public delegate void OnTimeUpdateHandler();

    [JSProperty("onended")]
    [JSCallback]
    public delegate void OnEndedHandler();

    public enum ReadyStates
    {
        HaveNothing = 0,
        HaveMetadata = 1,
        HaveCurrentData = 2,
        HaveFutureData = 3,
        HaveEnoughData
    }
}
