namespace BigTacToe

open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms

module App =
    let init  () = Types.initModel, Cmd.none

    // Remember MiniMax algorithm

    let view (model: Model) dispatch =
        let gameStatus =
            match model.Board.Winner with
            | None -> View.Label(text = sprintf "It is %s turn to play" (model.CurrentPlayer.ToString()))
            | Some w ->
                match w with
                | Draw -> View.Label(text = "It's a tie game. Nobody wins")
                | Player p -> View.Label(text = sprintf "%s wins!" (p.ToString()))

        let gameBoard =
            View.StackLayout
                (children = [
                 dependsOn (model.Size, model.Board) (fun _ (size, board) ->
                     View.SKCanvasView
                         (invalidate = true,
                          enableTouchEvents = true,
                          verticalOptions = LayoutOptions.FillAndExpand,
                          horizontalOptions = LayoutOptions.FillAndExpand,
                          paintSurface =
                              (fun args ->
                                  dispatch <| ResizeCanvas args.Info.Size
                         
                                  args.Surface.Canvas.Clear()
                         
                                  Render.drawMeeple args model.Board
                                  Render.drawBoard args board),
                          touch =
                              (fun args ->
                                  if args.InContact
                                  then dispatch (SKSurfaceTouched args.Location))))
                ])

        let page =
            let dimension = fst model.Size
            View.ContentPage
                (content =
                    View.Grid
                        (rowdefs = [Absolute 50.0; Star; Absolute 50.0],
                         coldefs = [Star],
                         padding = Thickness 20.0,
                         ref = model.GridLayout,
                         children = [
                            gameStatus.BackgroundColor(Color.Red)
                            gameBoard.Row(1)
                            View.Button(
                                text = dimension.ToString()
                            ).Row(2).BackgroundColor(Color.Green)
                         ]))

        page

    // Note, this declaration is needed if you enable LiveUpdate
    let program =
        Program.mkProgram init Messages.update view

type App() as app =
    inherit Application()

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
    //do runner.EnableLiveUpdate()
#endif
  

// Uncomment this code to save the application state to app.Properties using Newtonsoft.Json
// See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/models.html#saving-application-state for further  instructions.
#if APPSAVE
    let modelId = "model"

    override __.OnSleep() =

        let json =
            Newtonsoft.Json.JsonConvert.SerializeObject(runner.CurrentModel)

        Console.WriteLine("OnSleep: saving model into app.Properties, json = {0}", json)

        app.Properties.[modelId] <- json

    override __.OnResume() =
        Console.WriteLine "OnResume: checking for model in app.Properties"
        try
            match app.Properties.TryGetValue modelId with
            | true, (:? string as json) ->

                Console.WriteLine("OnResume: restoring model from app.Properties, json = {0}", json)

                let model =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<App.Model>(json)

                Console.WriteLine("OnResume: restoring model from app.Properties, model = {0}", (sprintf "%0A" model))
                runner.SetCurrentModel(model, Cmd.none)

            | _ -> ()
        with ex -> App.program.onError ("Error while restoring model found in app.Properties", ex)

    override this.OnStart() =
        Console.WriteLine "OnStart: using same logic as OnResume()"
        this.OnResume()
#endif
