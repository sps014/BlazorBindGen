## BlazorBindGen
  [![NuGet Package](https://img.shields.io/badge/nuget-v0.0.3%20Preview%204-orange.svg)](https://www.nuget.org/packages/BlazorBindGen/)
[![NuGet Badge](https://buildstats.info/nuget/BlazorBindGen)](https://www.nuget.org/packages/BlazorBindGen/)
![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)

A binding generator for JS, Call any JS function or property in <b>Blazor Wasm and Server</b> without writing JS wrappers.

#### Why Use BlazorBindGen

* very tiny Overhead  ~13kb
* No need to write JS wrappers
* Support for Callbacks
* Write JS code in C# 
* WASM and Server Supported
* automatic memory management


#### Installation
Use [Nuget Package Manager](https://www.nuget.org/packages/BlazorBindGen/) or .Net CLI 
```
dotnet add package BlazorBindGen
```

#### Initialize BindGen
1. on top of razor page add Import statements
```razor

@inject IJSRuntime runtime
@using BlazorBindGen
@using static BlazorBindGen.BindGen
@using JSCallBack=System.Action<BlazorBindGen.JObjPtr[]>; //optional Typedef 
```

2. Intitialize the BindGen
```cs
 protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await InitAsync(runtime);
    }
    //on Server 
    protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;
		await InitAsync(runtime);	
	}
```


#### Binding Samples

<b>
-> Js code is for explaination purpose only , you do not need to write js code anywhere.<br>
-> On Server use Async Version of functions (non async functions will throw `PlatformNotSupportedException`)</b>

##### Import JS libaries when ever you want in C#
```js  
// js equivalent
await import("https://unpkg.com/ml5@latest/dist/ml5.min.js");
//c# side
await ImportAsync("https://unpkg.com/ml5@latest/dist/ml5.min.js");
```

###### Constructor Calls
```cs
//js side
var audio=new Audio(param);

//c# side code 
var_audio=Window.Construct("Audio",param); /* js reference to Audio Player */ 
```

###### Fuction Calls

```cs

//js equivalent 
alert("Hello");

//code to call alert in C#
Window.CallVoid("alert","hello");
```


##### Share JS object References
```cs
//js equivalent 
var video = document.querySelector("#videoElement");
//here document is property of window , and dcument has function querySelector


//c# code 
var video = Window["document"].CallRef("querySelector", "#videoElement");
//["documemnt"] will return reference to Property document of window , another way to write it is 
JObjPtr video = Window.PropRef("document").CallRef("querySelector", "#videoElement");
//CallRef function calls JS function and Returns a reference to it, instead of whole object 
```


##### Get Set Properties
```cs
// equivalent js code 
var ctx = c.getContext("2d");
var grd = ctx.createRadialGradient(75, 50, 5, 90, 60, 100);
ctx.fillStyle = grd;
		
//c# side 
var ctx=canvas.CallRef("getContext","2d");
var grad = ctx.CallRef("createLinearGradient", 0,0,400,0);
ctx.SetPropRef("fillStyle",grad); 
//assign a reference to grad(a JobjPtr reference) to property fillStyle of canvas context
```


##### Mapping JS property to C#
```cs
//js
var audio=new Audio();
audio.currentTime=6; //set
console.log(audio.currentTime); //get

//c# equivalent
JObjPtr _audio=Window.Construct("Audio"); /* js reference to Audio Player */ 
public double CurrentTime
{
    get => _audio.PropVal<double>("currentTime");
    set => _audio.SetPropVal("currentTime", value);
}
```
##### Map Js Callback to C# event 
```cs
//js equivalent
var audio=new Audio();
audio.onloadeddata=()=>{ console.log("loaded"))};

//cs equivalent
{
   var _audio=Window.Construct("Audio"); /* js reference to Audio Player */ 
   _audio.SetPropCallBack("onloadedmetadata", (_) => OnLoadedMetaData?.Invoke(this));
}
public delegate void LoadedMetaDataHandler(object sender);
public event LoadedMetaDataHandler OnLoadedMetaData;
```

Be sure to check out sampleApp for more examples

#### Example (using ML5 in C# only)
```razor

@page "/ml5"

@using BlazorBindGen
@using static BlazorBindGen.BindGen
@using JSCallBack=System.Action<BlazorBindGen.JObjPtr[]>; //optional only needed to simplify callback type name

@inject IJSRuntime runtime

@if (isLoaded)
{
    <input type="text" class="bg-dark text-white border-light" @bind="predictText" placeholder="write review here " style="font-size:18px"/>
    <button class="btn btn-primary" id="mbtn" @onclick="Predict">Predict</button><br /><br />
    if(score>0)
    {
        <div class="alert alert-primary">
            <p>Review: @GetEmoji() <br />Score: @score</p>
        </div>
    }
}
else
{
    <div class="alert alert-warning">
        Fetching Movie Review Dataset (~16 MB)
    </div>
}
@code
{
    JWindow win;
    public JObjPtr sentiment;
    string predictText;
    bool isLoaded = false;
    double score;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await Init(runtime);
        win = Window;
        await ML5Init();
    }

    async Task ML5Init()
    {
        await Import("https://unpkg.com/ml5@latest/dist/ml5.min.js");
        Console.Clear();
        var ml5 = win["ml5"];
        sentiment = ml5.CallRef("sentiment", "movieReviews", (JSCallBack)OnModelLoaded);
    }

    void Predict()
    {
        var v = sentiment.Call<Score>("predict", predictText);
        score=v.score;
        StateHasChanged();
    }

    void OnModelLoaded(params JObjPtr[] args)
    {
        isLoaded = true;
        StateHasChanged();
    }

    string GetEmoji()
    {
        if (score > 0.7)
            return "😀";
        else if (score > 0.4)
            return "😐";
        else
            return "😥";

    }

    record Score(double score);

}


```




#### Warning 
1. BlazorBindGen Api is subject to change, API is not stable.
2. Note: Blazor Server requires use of Async functions otherwise UI thread will be blocked by it or alternatively you can call BindGen functions on different thread <br/>
[#issue](https://github.com/dotnet/aspnetcore/issues/37926).

