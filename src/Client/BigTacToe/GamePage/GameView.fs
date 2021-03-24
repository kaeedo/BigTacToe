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
                    [ dependsOn (model.Size, gm.Board, model.AnimatingMeeples) (fun _ _ ->
                          View.SKCanvasView
                              (invalidate = true,
                               ref = model.Canvas,
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

            match model.GameModel.Board.Winner with
            | Some (Participant p) ->
                let reason =
                    if model.OpponentStatus = Quit then " Opponent quit." else String.Empty

                if p.PlayerId = model.MyStatus.PlayerId then
                    [ View.Label
                        (text = sprintf "You Win!%s" reason,
                         fontSize = FontSize.Size 24.0,
                         horizontalTextAlignment = TextAlignment.Center) ]
                else
                    [ View.Label
                        (text = "You Lose :(",
                         fontSize = FontSize.Size 24.0,
                         horizontalTextAlignment = TextAlignment.Center) ]
            | Some (Draw) ->
                [ View.Label
                    (text = "It's a tie game",
                     fontSize = FontSize.Size 24.0,
                     horizontalTextAlignment = TextAlignment.Center) ]
            | None ->
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
                            [ Absolute 75.0
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
                               | Quit
                               | Joined _ ->
                                   yield! multiplayerText model
                                   gameBoard.Row(2)

                                   (View.FlexLayout
                                       (children =
                                           [ View.Button(text = "Main Menu", command = fun () -> dispatch GoToMainMenu) ]))
                                       .Row(3)
                               | WaitingForPrivate gameId ->
                                   match gameId with
                                   | None ->
                                       (View.Label
                                           (text = "Would you like to start a new private game or join a game?",
                                            fontSize = FontSize.Size 24.0,
                                            horizontalTextAlignment = TextAlignment.Center))
                                           .RowSpan(2)

                                       (View.Grid
                                           (rowdefs = [ Absolute 50.0; Absolute 50.0 ],
                                            coldefs = [ Stars 2.0; Star ],
                                            height = 100.0,
                                            children =
                                                [ View
                                                    .Button(text = "Start new game",
                                                            command = fun () -> dispatch StartPrivateGame)
                                                      .ColumnSpan(2)
                                                  View
                                                      .Entry(placeholder = "Game ID",
                                                             keyboard = Keyboard.Numeric,
                                                             textChanged =
                                                                 (fun args -> dispatch <| EnterGameId args.NewTextValue),
                                                             completed = fun text -> dispatch <| JoinPrivateGame text)
                                                      .Row(1)
                                                  View
                                                      .Button(text = "Join",
                                                              command =
                                                                  fun () -> dispatch <| JoinPrivateGame model.GameIdText)
                                                      .Row(1)
                                                      .Column(1) ]))
                                           .Row(2)
                                   | Some gameId ->
                                       (View.Label
                                           (text = sprintf "Tell your friend to join Game ID: %i" gameId,
                                            fontSize = FontSize.Size 24.0,
                                            horizontalTextAlignment = TextAlignment.Center))
                                           .RowSpan(2)

                                       View
                                           .StackLayout(height = 30.0,
                                                        children = [ View.ActivityIndicator(isRunning = true) ])
                                           .Row(2)
                               | _ ->
                                   View
                                       .Label(text = gameStatus,
                                              fontSize = FontSize.Size 24.0,
                                              horizontalTextAlignment = TextAlignment.Center)
                                       .RowSpan(2)

                                   gameBoard.Row(2) ]))

        page
