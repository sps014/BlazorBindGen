## BlazorBindGen
  [![NuGet Package](https://img.shields.io/badge/nuget-v0.0.1%20Preview%204-orange.svg)](https://www.nuget.org/packages/BlazorBindGen/)
[![NuGet Badge](https://buildstats.info/nuget/BlazorBindGen)](https://www.nuget.org/packages/BlazorBindGen/)
![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)

A binding generator for JS, Call any JS function or property in Blazor Wasm without writing JS wrappers.

#### Why Use BlazorBindGen

* very tiny Overhead  ~13kb
* No need to write JS wrappers
* Support for Callbacks and parameters
* Write JS code in C# 
* performant


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
        await Init(runtime);
    }
```


#### Binding Samples

###### Fuction Calls
you can call any function or set, get any property of js  by using JS Object Reference (JObjPtr) ,everything managed cleaned up automatically.
*** Js code is for explaination purpose only , you do not need to write it anywhere

```cs

//js equivalent  (no need to write this , c# code autogenerates it)
alert("Hello");

//code to call alert in C#
Window.CallVoid("alert","hello");
```

##### Share JS object References
```cs
//js equivalent (no need to write this , c# code autogenerates it)
var video = document.querySelector("#videoElement");
//here document is property of window , and dcument has function querySelector
//c# code 
var video = Window["document"].CallRef("querySelector", "#videoElement");
//["documemnt"] will return reference to Property document of window , another way to write it is 
var video = Window.PropRef("document").CallRef("querySelector", "#videoElement");
```


#### Example (using Audio Player from JS)
```cs

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
BlazorBindGen Api are subject to change.
