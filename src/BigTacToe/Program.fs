namespace BigTacToe

open System.Diagnostics
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms
open SkiaSharp
open SkiaSharp.Views.Forms

module App =
    let init () = Types.initModel, Cmd.none

    let view (model: Model) dispatch =
        let subBoard = 
            model.Board.SubBoards
            |> Seq.cast<SubBoard>
            |> Seq.tryFind (fun sb ->
                sb.Rect.Contains(model.TouchPoint)
            )

        let touchedTile =
            subBoard
            |> Option.bind (fun sb ->
                sb.Tiles
                |> Seq.cast<SKRect * (Meeple option)>
                |> Seq.tryFind (fun (rect, m) ->
                    rect.Contains(model.TouchPoint)
                )
            )

        let page =
            View.ContentPage(
              content = View.StackLayout(padding = Thickness 20.0,
                verticalOptions = LayoutOptions.FillAndExpand,
                ref = model.StackLayout,
                children = [
                    dependsOn (model.Board, model.TouchPoint) (fun _ (board, touchPoint) ->
                        View.SKCanvasView(
                            invalidate = true,
                            enableTouchEvents = true,
                            paintSurface = (fun args ->
                                dispatch <| ResizeCanvas args.Info.Size

                                args.Surface.Canvas.Clear()
                                
                                SKBoard.drawBoard args board

                                if touchedTile.IsSome
                                then SKBoard.highlightSquare args touchedTile.Value
                            ),
                            horizontalOptions = LayoutOptions.FillAndExpand,
                            verticalOptions = LayoutOptions.FillAndExpand,
                            touch = (fun args ->
                                if args.InContact then
                                    dispatch (SKSurfaceTouched args.Location)
                        ))
                    )
                    
                    View.Label(text = sprintf "touched X: %f Y: %f" model.TouchPoint.X model.TouchPoint.Y, horizontalOptions = LayoutOptions.Center, width=200.0, horizontalTextAlignment=TextAlignment.Center)
                    if subBoard.IsSome
                    then View.Label(text = sprintf "touched SubBoard: %A" subBoard.Value.Rect.Location, horizontalOptions = LayoutOptions.Center, width=200.0, horizontalTextAlignment=TextAlignment.Center)

                    if touchedTile.IsSome
                    then View.Label(text = sprintf "touched Tile: %A" (fst touchedTile.Value).Location, horizontalOptions = LayoutOptions.Center, width=200.0, horizontalTextAlignment=TextAlignment.Center)
                    ]))
        page

    // Note, this declaration is needed if you enable LiveUpdate
    let program = Program.mkProgram init Messages.update view

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


