// Copyright 2018-2019 Fabulous contributors. See LICENSE.md for license.
namespace MqueakyApp

open System.Diagnostics
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms
open SkiaSharp
open SkiaSharp.Views.Forms

module App = 
    type Model = 
      { Count : int
        Step : int
        AnimationShouldRun : bool
        StackLayout : ViewRef<StackLayout>
        TimerOn: bool }

    type Msg = 
        | Increment 
        | Decrement 
        | Reset
        | SetStep of int
        | TimerToggled of bool
        | TimedTick
        | SKSurfaceTouched of SKPoint
        | AnimationShouldRun of bool

    let initModel = { Count = 0; Step = 1; TimerOn=false; AnimationShouldRun = true; StackLayout = ViewRef<StackLayout>() }

    let init () = initModel, Cmd.none

    let timerCmd =
        async { do! Async.Sleep 200
                return TimedTick }
        |> Cmd.ofAsyncMsg

    let update msg model =
        match msg with
        | Increment -> { model with Count = model.Count + model.Step }, Cmd.none
        | Decrement -> { model with Count = model.Count - model.Step }, Cmd.none
        | AnimationShouldRun shouldRun -> { model with AnimationShouldRun = shouldRun }, Cmd.none
        | Reset -> init ()
        | SetStep n -> { model with Step = n }, Cmd.none
        | TimerToggled on -> { model with TimerOn = on }, (if on then timerCmd else Cmd.none)
        | SKSurfaceTouched _ ->
            model, Cmd.none
        | TimedTick -> 
            if model.TimerOn then 
                { model with Count = model.Count + model.Step }, timerCmd
            else 
                model, Cmd.none

    let view (model: Model) dispatch =
        let animate = new Animation((fun f -> dispatch Increment), start = 0.0, ``end``= 1.0, easing = Easing.BounceIn)
        let sk = ViewRef<SKCanvas>()

        let page = 
            View.ContentPage(
              content = View.StackLayout(padding = Thickness 20.0, 
                verticalOptions = LayoutOptions.FillAndExpand,
                ref = model.StackLayout,
                children = [ 
                    View.SKCanvasView(
                        //ref = sk,
                        invalidate = true,
                        enableTouchEvents = true, 
                        paintSurface = (fun args -> 
                            let info = args.Info
                            let surface = args.Surface
                            let canvas = surface.Canvas

                            canvas.Clear() 
                            use paint = new SKPaint(Style = SKPaintStyle.Stroke, Color = Color.Blue.ToSKColor(), StrokeWidth = 25.0f)
                            canvas.DrawCircle(float32 (info.Width / 2), float32 (info.Height / 2), float32 (model.Count), paint)
                        ),
                        horizontalOptions = LayoutOptions.FillAndExpand, 
                        verticalOptions = LayoutOptions.FillAndExpand, 
                        touch = (fun args -> 
                            if args.InContact then
                                dispatch (SKSurfaceTouched args.Location)
                        ))
                    View.Label(text = sprintf "%d" model.Count, horizontalOptions = LayoutOptions.Center, width=200.0, horizontalTextAlignment=TextAlignment.Center)
                    View.Button(text = "Increment", command = (fun () -> dispatch Increment), horizontalOptions = LayoutOptions.Center)
                    View.Button(text = "Decrement", command = (fun () -> dispatch Decrement), horizontalOptions = LayoutOptions.Center)
                    View.Label(text = "Timer", horizontalOptions = LayoutOptions.Center)
                    View.Switch(isToggled = model.TimerOn, toggled = (fun on -> dispatch (TimerToggled on.Value)), horizontalOptions = LayoutOptions.Center)
                    View.Slider(minimumMaximum = (0.0, 10.0), value = double model.Step, valueChanged = (fun args -> dispatch (SetStep (int (args.NewValue + 0.5)))), horizontalOptions = LayoutOptions.FillAndExpand)
                    View.Label(text = sprintf "Step size: %d" model.Step, horizontalOptions = LayoutOptions.Center) 
                    View.Button(text = "Reset", horizontalOptions = LayoutOptions.Center, command = (fun () -> dispatch Reset), commandCanExecute = (model <> initModel))
                ]))

        match model.StackLayout.TryValue with
        | None -> ()
        | Some c ->
            if (not (c.AnimationIsRunning("HomePage"))) && model.AnimationShouldRun
            then animate.Commit(c, "HomePage", rate = 1000u, length = 5000u, finished = (System.Action<float, bool>(fun a b -> 
                                                                                            dispatch <| AnimationShouldRun false
                                                                                            ()
                                                                                        ))
                )
        page

    // Note, this declaration is needed if you enable LiveUpdate
    let program = Program.mkProgram init update view

type App () as app = 
    inherit Application ()

    let runner = 
        App.program
#if DEBUG
        |> Program.withConsoleTrace
#endif
        |> XamarinFormsProgram.run app

#if DEBUG
    // Uncomment this line to enable live update in debug mode. 
    // See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/tools.html#live-update for further  instructions.
    //
    do runner.EnableLiveUpdate()
#endif    

    // Uncomment this code to save the application state to app.Properties using Newtonsoft.Json
    // See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/models.html#saving-application-state for further  instructions.
#if APPSAVE
    let modelId = "model"
    override __.OnSleep() = 

        let json = Newtonsoft.Json.JsonConvert.SerializeObject(runner.CurrentModel)
        Console.WriteLine("OnSleep: saving model into app.Properties, json = {0}", json)

        app.Properties.[modelId] <- json

    override __.OnResume() = 
        Console.WriteLine "OnResume: checking for model in app.Properties"
        try 
            match app.Properties.TryGetValue modelId with
            | true, (:? string as json) -> 

                Console.WriteLine("OnResume: restoring model from app.Properties, json = {0}", json)
                let model = Newtonsoft.Json.JsonConvert.DeserializeObject<App.Model>(json)

                Console.WriteLine("OnResume: restoring model from app.Properties, model = {0}", (sprintf "%0A" model))
                runner.SetCurrentModel (model, Cmd.none)

            | _ -> ()
        with ex -> 
            App.program.onError("Error while restoring model found in app.Properties", ex)

    override this.OnStart() = 
        Console.WriteLine "OnStart: using same logic as OnResume()"
        this.OnResume()
#endif


