namespace BigTacToe.Pages

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
        
        let canvasView size gameModel runningAnimation canvas =
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
                       then dispatch (SKSurfaceTouched args.Location)))

        let gameBoard =
            StackLayout.stackLayout [
                StackLayout.Row 2
                StackLayout.Children [
                    dependsOn (model.Size, gm, model.RunningAnimation, model.Canvas) (fun model (size, gameModel, runningAnimation, canvas) ->
                        canvasView size gameModel runningAnimation canvas
                        )
                ]
            ]

        let gameText model =
            match model.GameModel.Board.Winner with
            | Some (Participant p) ->
                let winnerText =
                    if model.OpponentStatus = LocalGame
                    then sprintf "%s wins!" <| p.Meeple.ToString()
                    else
                        if p.PlayerId = model.MyStatus.PlayerId
                        then if model.OpponentStatus = Quit then "You Won! Opponent quit." else "You Win!"
                        else "You Lost :("

                Label.label [
                    Label.Text winnerText
                    Label.FontSize <| FontSize.Size 24.0
                    Label.HorizontalTextAlignment TextAlignment.Center
                ]
            | Some (Draw) ->
                Label.label [
                    Label.Text "It's a tie game"
                    Label.FontSize <| FontSize.Size 24.0
                    Label.HorizontalTextAlignment TextAlignment.Center
                ]
            | None ->
                let turnToPlay =
                    if model.OpponentStatus = LocalGame
                    then sprintf "It is %s's turn to play" <| model.GameModel.CurrentPlayer.Meeple.ToString()
                    else
                        let opponentMeeple =
                            if model.MyStatus.Meeple = Meeple.Ex then Meeple.Oh else Meeple.Ex

                        let yourTurnText = "Your turn"

                        let waitingForOpponent = sprintf "Waiting for %s" <| opponentMeeple.ToString()

                        if model.GameModel.CurrentPlayer.PlayerId = model.MyStatus.PlayerId
                        then yourTurnText
                        else waitingForOpponent
                Label.label [
                    Label.Text turnToPlay
                    Label.FontSize <| FontSize.Size 24.0
                    Label.HorizontalTextAlignment TextAlignment.Center
                ]
                
        let spinner =
            StackLayout.stackLayout [
                StackLayout.Height 30.0
                StackLayout.Row 2
                StackLayout.Children [
                    ActivityIndicator.activityIndicator [
                        ActivityIndicator.IsRunning true
                    ]
                ]
            ]

        let waitingForPrivate gameId model =
            match gameId with
            | None ->
                [ Label.label [
                      Label.Text "Would you like to start a new private game or join a game?"
                      Label.FontSize <| FontSize.Size 24.0
                      Label.HorizontalTextAlignment TextAlignment.Center
                      Label.RowSpan 2
                  ]
                
                  Grid.grid [
                      Grid.Rows [ Absolute 50.0; Absolute 50.0 ]
                      Grid.Columns [ Stars 2.0; Star ]
                      Grid.Height 100.0
                      Grid.Row 2
                      Grid.Children [
                          Button.button [
                              Button.Text "Start new game"
                              Button.ColumnSpan 2
                              Button.OnClick (fun () -> dispatch StartPrivateGame)
                          ]
                          TextEntry.textEntry [
                              TextEntry.Placeholder "Game ID"
                              TextEntry.Keyboard Keyboard.Numeric
                              TextEntry.Row 1
                              TextEntry.OnTextChanged (fun args -> dispatch <| EnterGameId args.NewTextValue)
                              TextEntry.OnCompleted (fun text -> dispatch <| JoinPrivateGame text)
                          ]
                          Button.button [
                              Button.Text "Join"
                              Button.Row 1
                              Button.Column 1
                              Button.OnClick (fun () -> dispatch <| JoinPrivateGame model.GameIdText)
                          ]
                      ]
                  ]
                ]
            | Some gameId ->
                [ Label.label [
                      Label.Text <| sprintf "Tell your friend to join Game ID: %i" gameId
                      Label.FontSize <| FontSize.Size 24.0
                      Label.HorizontalTextAlignment TextAlignment.Center
                      Label.RowSpan 2
                  ]
                  
                  spinner
                ]
                
        let footerNavigation =
            FlexLayout.flexLayout [
                FlexLayout.Row 3
                FlexLayout.Children [
                    Button.button [
                        Button.Text "Main Menu"
                        Button.OnClick (fun () -> dispatch GoToMainMenu)
                    ]
                ]
            ]

        let children model =
            match model.OpponentStatus with
            | LookingForGame ->
                [ Label.label [
                      Label.Text "Looking for game"
                      Label.FontSize <| FontSize.Size 24.0
                      Label.HorizontalTextAlignment TextAlignment.Center
                  ]
                  spinner
                ]
            | WaitingForPrivate gameId -> waitingForPrivate gameId model
            | Quit
            | LocalAiGame
            | LocalGame
            | Joined _ ->
                [ gameText model
                  gameBoard
                  footerNavigation ]

        let page =
            ContentPage.contentPage [
                ContentPage.HasNavigationBar false
                ContentPage.Content <|
                    Grid.grid [
                    Grid.Rows [ Absolute 75.0; Absolute 50.0; Star; Absolute 50.0 ]
                    Grid.Columns [Star]
                    Grid.Padding 20.0
                    Grid.Children <| children model
                ]
            ]

        page
