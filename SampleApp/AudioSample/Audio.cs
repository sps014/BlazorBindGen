using System;
using Microsoft.JSInterop;
using BlazorBindGen;
using System.Threading.Tasks;

namespace BlazorApp
{
    public class Audio
    {
        //refers to object in JS
        private readonly JObjPtr _audio;
        
        //js property to c#
        public double Duration
        {
            get
            {
                //duration is undefined sometimes hence try block
                try
                {
                    //get property duration of audio
                    return _audio.PropVal<double>("duration");
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
        //map js audio currentTime to c# property
        public double CurrentTime
        {
            get => _audio.PropVal<double>("currentTime");
            set => _audio.SetPropVal("currentTime", value);
        }
        //map js audio readyState property enum result to c#
        public ReadyStates ReadyState => (ReadyStates)_audio.PropVal<int>("readyState");

        public bool Paused => _audio.PropVal<bool>("paused");

        public double Volume
        {
            get => _audio.PropVal<double>("volume");
            set => _audio.SetPropVal("volume", value);
        }
        public bool Muted
        {
            get => _audio.PropVal<bool>("muted");
            set => _audio.SetPropVal("muted", value);
        }

        public Audio(string url = null)
        {
            //equivalent to `let _audio=new Audio(url); in js`
            _audio = url != null ? BindGen.Window.Construct("Audio", url) : BindGen.Window.Construct("Audio");
            
            //subscribe to callback c# side (map to OnCanPlayEvent) 
            _audio.SetPropCallBack("oncanplay", (_) => OnCanPlay?.Invoke(this));
            _audio.SetPropCallBack("ontimeupdate", (_) => OnTimeUpdate?.Invoke(this));
            _audio.SetPropCallBack("onloadedmetadata", (_) => OnLoadedMetaData?.Invoke(this));
            _audio.SetPropCallBack("onended", (_) => OnEnded?.Invoke(this));

        }

        public static async ValueTask Init(IJSRuntime runtime = null)
        {
            //initialize the core libary 
            if (runtime != null)
                await BindGen.InitAsync(runtime);
        }
        
        
        //call js setsource on _audio in js
        public void SetSource(string url)
        {
            _audio.SetPropVal("src", url);
        }
        //call js pause() on _audio object
        public void Pause()
        {
            _audio.CallVoid("pause");
        }
        
         //call js play() on _audio object ; js side equivalent await _audio.play(); 

        public async ValueTask Play()
        {
            await _audio.CallVoidAwaitedAsync("play");
        }
        
        //onmetadatacallback 
        public delegate void LoadedMetaDataHandler(object sender);
        public event LoadedMetaDataHandler OnLoadedMetaData;
        
        public delegate void OnCanPlayHandler(object sender);
        public event OnCanPlayHandler OnCanPlay;
        
        public delegate void OnTimeUpdateHandler(object sender);
        public event OnTimeUpdateHandler OnTimeUpdate;
        
        public delegate void OnEndedHandler(object sender);
        public event OnEndedHandler OnEnded;

        public enum ReadyStates
        {
            HaveNothing = 0,
            HaveMetadata = 1,
            HaveCurrentData = 2,
            HaveFutureData = 3,
            HaveEnoughData
        }
    }
}
