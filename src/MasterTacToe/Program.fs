namespace MasterTacToe

open System.Diagnostics
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms
open SkiaSharp
open SkiaSharp.Views.Forms

module App = 
    type Model = 
      { TouchPoint: SKPoint
        StackLayout : ViewRef<StackLayout> }

    type Msg = 
        | SKSurfaceTouched of SKPoint

    let initModel = { StackLayout = ViewRef<StackLayout>(); TouchPoint = SKPoint.Empty }

    let init () = initModel, Cmd.none

    let update msg model =
        match msg with
        | SKSurfaceTouched point -> { model with TouchPoint = point }, Cmd.none

    let view (model: Model) dispatch =
        let page = 
            View.ContentPage(
              content = View.StackLayout(padding = Thickness 20.0, 
                verticalOptions = LayoutOptions.FillAndExpand,
                ref = model.StackLayout,
                children = [ 
                    View.SKCanvasView(
                        invalidate = true,
                        enableTouchEvents = true, 
                        paintSurface = (fun args -> 
                            let info = args.Info
                            let surface = args.Surface
                            let canvas = surface.Canvas

                            canvas.Clear() 
                            Board.drawBoard args
                        ),
                        horizontalOptions = LayoutOptions.FillAndExpand, 
                        verticalOptions = LayoutOptions.FillAndExpand, 
                        touch = (fun args -> 
                            if args.InContact then
                                dispatch (SKSurfaceTouched args.Location)
                        ))
                    ]))
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


