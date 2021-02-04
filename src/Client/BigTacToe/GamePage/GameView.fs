namespace BigTacToe.Pages

open System
open BigTacToe.Pages

open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.SkiaSharp
open Xamarin.Forms
open BigTacToe.Shared

[<RequireQualifiedAccess>]
module internal Game =
    let view (model: ClientGameModel) dispatch =
        let gm = model.GameModel

        let gameStatus =
            match gm.Board.Winner with
            | None -> sprintf "It is %s turn to play" (gm.CurrentPlayer.ToString())
            | Some w ->
                match w with
                | Draw -> "It's a tie game. Nobody wins"
                | Participant p -> sprintf "%s wins!" (p.ToString())


        let gameBoard =
            View.StackLayout
                (children =
                    [ dependsOn (model.Size, gm.Board) (fun _ (size, board) ->
                          View.SKCanvasView
                              (invalidate = true,
                               enableTouchEvents = true,
                               verticalOptions = LayoutOptions.FillAndExpand,
                               horizontalOptions = LayoutOptions.FillAndExpand,
                               paintSurface =
                                   (fun args ->
                                       dispatch <| ResizeCanvas args.Info.Size

                                       args.Surface.Canvas.Clear()

                                       Render.drawBoard args model
                                       Render.drawMeeple args model),
                               touch =
                                   (fun args ->
                                       if args.InContact
                                       then dispatch (SKSurfaceTouched args.Location)))) ])

        let multiplayerText model =
            let iAm =
                sprintf
                    "I am %s, with ID: %s"
                    (model.MyStatus.Meeple.ToString())
                    (model.MyStatus.PlayerId.ToString().Substring(0, 6))

            let turnToPlay =
                sprintf
                    "It is%s my turn to play"
                    (if model.GameModel.CurrentPlayer.PlayerId = model.MyStatus.PlayerId
                     then String.Empty
                     else " not")

            [ View.Label(text = iAm, fontSize = FontSize.Size 24.0, horizontalTextAlignment = TextAlignment.Center)
              View
                  .Label(text = turnToPlay,
                         fontSize = FontSize.Size 24.0,
                         horizontalTextAlignment = TextAlignment.Center)
                  .Row(1) ]

        let page =
            View.ContentPage
                (content =
                    View.Grid
                        (rowdefs =
                            [ Absolute 50.0
                              Absolute 50.0
                              Star
                              Absolute 50.0 ],
                         coldefs = [ Star ],
                         padding = Thickness 20.0,
                         children =
                             [ match model.OpponentStatus with
                               | LookingForGame ->
                                   View
                                       .Label(text = "Looking for game",
                                              fontSize = FontSize.Size 24.0,
                                              horizontalTextAlignment = TextAlignment.Center)
                                       .RowSpan(2)

                                   View
                                       .StackLayout(height = 30.0,
                                                    children = [ View.ActivityIndicator(isRunning = true) ])
                                       .Row(2)
                               | Joined p ->
                                   yield! multiplayerText model
                                   gameBoard.Row(2)

                                   (View.FlexLayout
                                       (children =
                                           [ View.Button(text = "Main Menu", command = fun () -> dispatch GoToMainMenu) ])).Row(3)
                               | _ ->
                                   View
                                       .Label(text = gameStatus,
                                              fontSize = FontSize.Size 16.0,
                                              horizontalTextAlignment = TextAlignment.Center)
                                       .RowSpan(2)

                                   gameBoard.Row(2) ]))

        page
