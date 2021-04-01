namespace BigTacToe.Pages

open BigTacToe.Pages

open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.SkiaSharp
open Xamarin.Forms
open BigTacToe.Shared
open Xamarin.Forms

[<RequireQualifiedAccess>]
module internal Game =
    let view (model: ClientGameModel) dispatch =
        let gm = model.GameModel

        let gameBoard =
            View.StackLayout
                (children =
                    [ dependsOn (model.Size, gm, model.RunningAnimation, model.Canvas) (fun model (size, gameModel, runningAnimation, canvas) ->
                          View.SKCanvasView
                              (invalidate = true,
                               ref = canvas,
                               enableTouchEvents = true,
                               verticalOptions = LayoutOptions.FillAndExpand,
                               horizontalOptions = LayoutOptions.FillAndExpand,
                               paintSurface =
                                   (fun args ->
                                       dispatch <| ResizeCanvas args.Info.Size

                                       args.Surface.Canvas.Clear()

                                       let hasGameStarted =
                                           gameModel.GameMoves |> List.isEmpty |> not

                                       Render.drawBoard args gameModel.Board size

                                       if hasGameStarted then Render.drawMeeple args gameModel runningAnimation size 

                                       if gameModel.Board.Winner.IsNone
                                          && hasGameStarted then
                                           Render.drawHighlights args gameModel size

                                       Render.drawWinners args gameModel runningAnimation size
                                       Render.startAnimations args gameModel runningAnimation size),
                               touch =
                                   (fun args ->
                                       if args.InContact
                                       then dispatch (SKSurfaceTouched args.Location)))) ])

        let multiplayerText model =
            let turnToPlay =
                let opponentMeeple =
                    if model.MyStatus.Meeple = Meeple.Ex then Meeple.Oh else Meeple.Ex

                let yourTurnText = "Your turn"

                let waitingForOpponent = sprintf "Waiting for %s" <| opponentMeeple.ToString()

                if model.GameModel.CurrentPlayer.PlayerId = model.MyStatus.PlayerId
                then yourTurnText
                else waitingForOpponent

            match model.GameModel.Board.Winner with
            | Some (Participant p) ->
                let winnerText =
                    if p.PlayerId = model.MyStatus.PlayerId
                    then if model.OpponentStatus = Quit then "You Won! Opponent quit." else "You Win!"
                    else "You Lost :("

                [ View.Label
                    (text = winnerText, fontSize = FontSize.Size 24.0, horizontalTextAlignment = TextAlignment.Center) ]
            | Some (Draw) ->
                [ View.Label
                    (text = "It's a tie game",
                     fontSize = FontSize.Size 24.0,
                     horizontalTextAlignment = TextAlignment.Center) ]
            | None ->
                [ View.Label
                    (text = turnToPlay, fontSize = FontSize.Size 24.0, horizontalTextAlignment = TextAlignment.Center) ]

        let localGameText model =
            let currentMeeple = model.GameModel.CurrentPlayer.Meeple

            let turnToPlay =
                sprintf "It is %s's turn to play"
                <| currentMeeple.ToString()

            match model.GameModel.Board.Winner with
            | Some (Participant p) ->
                let winnerText =
                    sprintf "%s wins!" <| p.Meeple.ToString()

                [ View.Label
                    (text = winnerText, fontSize = FontSize.Size 24.0, horizontalTextAlignment = TextAlignment.Center) ]
            | Some (Draw) ->
                [ View.Label
                    (text = "It's a tie game",
                     fontSize = FontSize.Size 24.0,
                     horizontalTextAlignment = TextAlignment.Center) ]
            | None ->
                [ View.Label
                    (text = turnToPlay, fontSize = FontSize.Size 24.0, horizontalTextAlignment = TextAlignment.Center) ]

        let waitingForPrivate gameId model =
            match gameId with
            | None ->
                [ (View.Label
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
                               .Button(text = "Start new game", command = fun () -> dispatch StartPrivateGame)
                                 .ColumnSpan(2)
                             View
                                 .Entry(placeholder = "Game ID",
                                        keyboard = Keyboard.Numeric,
                                        textChanged = (fun args -> dispatch <| EnterGameId args.NewTextValue),
                                        completed = fun text -> dispatch <| JoinPrivateGame text)
                                 .Row(1)
                             View
                                 .Button(text = "Join", command = fun () -> dispatch <| JoinPrivateGame model.GameIdText)
                                 .Row(1)
                                 .Column(1) ]))
                      .Row(2) ]
            | Some gameId ->
                [ (View.Label
                    (text = sprintf "Tell your friend to join Game ID: %i" gameId,
                     fontSize = FontSize.Size 24.0,
                     horizontalTextAlignment = TextAlignment.Center))
                    .RowSpan(2)

                  (View.StackLayout(height = 30.0, children = [ View.ActivityIndicator(isRunning = true) ]))
                      .Row(2) ]
                
        let footerNavigation =
            (View.FlexLayout
                    (children = [ View.Button(text = "Main Menu", command = fun () -> dispatch GoToMainMenu) ]))
                    .Row(3)

        let children model =
            match model.OpponentStatus with
            | LookingForGame ->
                [ View
                    .Label(text = "Looking for game",
                           fontSize = FontSize.Size 24.0,
                           horizontalTextAlignment = TextAlignment.Center)
                      .RowSpan(2)

                  View
                      .StackLayout(height = 30.0, children = [ View.ActivityIndicator(isRunning = true) ])
                      .Row(2) ]
            | Quit
            | Joined _ ->
                [ yield! multiplayerText model
                  gameBoard.Row(2)

                  footerNavigation ]
            | WaitingForPrivate gameId -> waitingForPrivate gameId model
            | LocalAiGame ->
                [ yield! multiplayerText model
                  gameBoard.Row(2)
                  footerNavigation ]
            | LocalGame ->
                [ yield! localGameText model
                  gameBoard.Row(2)
                  footerNavigation ]

        let grid =
            View.Grid
                (rowdefs =
                    [ Absolute 75.0
                      Absolute 50.0
                      Star
                      Absolute 50.0 ],
                 coldefs = [ Star ],
                 padding = Thickness 20.0,
                 children = children model)
                
        let page =
            (View.ContentPage
                (content = grid))
                .HasNavigationBar(false)

        page
