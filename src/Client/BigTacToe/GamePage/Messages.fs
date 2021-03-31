namespace BigTacToe.Pages

open System
open Fabulous
open SkiaSharp
open BigTacToe.Shared
open BigTacToe.Shared.SignalRHub
open Fable.SignalR.Elmish
open Xamarin.Forms

module internal Messages =
    let private zoomLevel = 0.97f

    let private calculatePositionPlayed (model: ClientGameModel) (point: SKPoint) =
        let size = model.Size

        let offset =
            let difference =
                (float32 model.Size / zoomLevel)
                - float32 model.Size

            difference / 2.0f

        let subBoardSize = size / 3

        let rects =
            Array2D.init 3 3 (fun i j ->
                let left = subBoardSize * i
                let top = subBoardSize * j
                let right = left + subBoardSize
                let bottom = top + subBoardSize

                let rect =
                    SKRect(float32 left + offset, float32 top + offset, float32 right + offset, float32 bottom + offset)

                rect,
                Array2D.init 3 3 (fun tileI tileJ ->
                    let tileSize = rect.Width / 3.0f |> int

                    let left = rect.Left + float32 (tileSize * tileI)
                    let right = left + float32 tileSize
                    let top = rect.Top + float32 (tileSize * tileJ)
                    let bottom = top + float32 tileSize

                    SKRect(left, top, right, bottom)))

        let (sbi, sbj) =
            rects
            |> Array2D.findIndexBy (fun (r: SKRect, _) -> r.Contains(point))

        let subBoardTiles = rects.[sbi, sbj] |> snd

        let (ti, tj) =
            subBoardTiles
            |> Array2D.findIndexBy (fun (r: SKRect) -> r.Contains(point))

        (sbi, sbj), (ti, tj)

    let private tryPlayPosition model gm positionPlayed =
        match GameRules.tryPlayPosition gm positionPlayed with
        | None -> (model, Cmd.none, GameExternalMsg.NoOp)
        | Some subBoard ->
            let gameMove =
                { GameMove.Player = model.GameModel.CurrentPlayer
                  PositionPlayed = positionPlayed }

            let newGm =
                GameRules.updateModel gm subBoard gameMove

            let animation = DrawingAnimation.init (GameMove gameMove)

            let createAnimationCommand =
                Cmd.ofSub (fun dispatch -> Animations.create animation (AnimationMessage >> dispatch))

            let command =
                match model.OpponentStatus with
                | Joined _ -> Cmd.SignalR.send model.Hub (Action.MakeMove(model.GameId, gameMove))
                | LocalGame -> Cmd.none
                | LocalAiGame ->
                    if newGm.Board.Winner.IsSome
                    then Cmd.none
                    else Cmd.ofAsyncMsg <| AiPlayer.playPosition newGm
                | _ -> Cmd.none

            let command =
                Cmd.batch ([ createAnimationCommand; command ])

            ({ model with
                   GameModel = newGm
                   RunningAnimation = Some animation },
             command,
             GameExternalMsg.NoOp)

    let update msg (model: ClientGameModel) =
        let gm = model.GameModel

        match msg with
        | ConnectToServer ->
            let cmd =
                Cmd.SignalR.connect RegisterHub (fun hub ->
                    hub
                        .WithUrl(sprintf "http://127.0.0.1:5000%s" Endpoints.Root)
                        .WithAutomaticReconnect()
                        .UseMessagePack()
                        .OnMessage SignalRMessage)

            model, cmd, GameExternalMsg.NoOp
        | RegisterHub hub ->
            let hub = Some hub

            let playerId = model.GameModel.CurrentPlayer.PlayerId

            let cmd =
                match model.OpponentStatus with
                | LookingForGame -> Cmd.SignalR.send hub (Action.OnConnect playerId)
                | WaitingForPrivate _ -> Cmd.SignalR.send hub (Action.OnConnect playerId)
                | _ -> Cmd.none

            { model with Hub = hub }, cmd, GameExternalMsg.NoOp
        | SignalRMessage response -> SignalRMessages.handleSignalRMessage model response
        | AnimationMessage message -> AnimationMessages.handleAnimationMessage model message
        | ResizeCanvas size ->
            let smallerDimension =
                if size.Width < size.Height then size.Width else size.Height

            { model with
                  Size = (float32 smallerDimension * zoomLevel) |> int },
            Cmd.none,
            GameExternalMsg.NoOp
        | OpponentPlayed positionPlayed ->
            let subBoards =
                GameRules.tryPlayPosition gm positionPlayed

            match subBoards with
            | Some sb ->
                let gameMove =
                    { GameMove.Player = gm.CurrentPlayer
                      PositionPlayed = positionPlayed }

                let newGm = GameRules.updateModel gm sb gameMove
                
                let animation =
                    DrawingAnimation.init (GameMove gameMove)

                let createAnimationCommand =
                    Cmd.ofSub (fun dispatch -> Animations.create animation (AnimationMessage >> dispatch))

                let model = { model with GameModel = newGm }

                model, createAnimationCommand, GameExternalMsg.NoOp
            | None -> model, Cmd.none, GameExternalMsg.NoOp // TODO: FIX THIS

        | StartPrivateGame ->
            let playerId = model.GameModel.CurrentPlayer.PlayerId

            let cmd =
                Cmd.SignalR.send model.Hub (Action.HostPrivateGame playerId)

            model, cmd, GameExternalMsg.NoOp

        | EnterGameId text ->
            let model = { model with GameIdText = text }
            model, Cmd.none, GameExternalMsg.NoOp

        | JoinPrivateGame text ->
            let playerId = model.GameModel.CurrentPlayer.PlayerId
            let (isSuccess, gameId) = Int32.TryParse(text)

            if isSuccess then
                let cmd =
                    Cmd.SignalR.send model.Hub (Action.JoinPrivateGame(gameId, playerId))

                model, cmd, GameExternalMsg.NoOp
            else
                model, Cmd.none, GameExternalMsg.NoOp

        | SKSurfaceTouched point when (model.OpponentStatus = LocalGame)
                                      && model.RunningAnimation.IsNone
                                      && gm.Board.Winner.IsNone ->
            let positionPlayed = calculatePositionPlayed model point

            tryPlayPosition model gm positionPlayed
        | SKSurfaceTouched point when (model.OpponentStatus <> LocalGame)
                                      && model.RunningAnimation.IsNone
                                      && (gm.CurrentPlayer.PlayerId = model.MyStatus.PlayerId)
                                      && gm.Board.Winner.IsNone ->
            let positionPlayed = calculatePositionPlayed model point

            tryPlayPosition model gm positionPlayed

        | GoToMainMenu ->
            if model.GameModel.Board.Winner.IsSome
            then model, Cmd.none, GameExternalMsg.NavigateToMainMenu
            else model, (Cmd.ofMsg DisplayGameQuitAlert), GameExternalMsg.NoOp
        | DisplayGameQuitAlert ->
            let alertResult =
                async {
                    let! confirmation =
                        Application.Current.MainPage.DisplayAlert
                            ("Quit Game", "Are you sure you want to quit this game and return to the menu?", "Yes", "No")
                        |> Async.AwaitTask

                    return GameQuitAlertResult confirmation
                }

            model, Cmd.ofAsyncMsg alertResult, GameExternalMsg.NoOp
        | GameQuitAlertResult isSure ->
            if isSure then
                model,
                Cmd.SignalR.send model.Hub (Action.QuitGame(model.GameId, model.MyStatus.PlayerId)),
                GameExternalMsg.NavigateToMainMenu
            else
                model, Cmd.none, GameExternalMsg.NoOp
        | ReturnToMainMenu -> model, Cmd.none, GameExternalMsg.NavigateToMainMenu
        | _ -> (model, Cmd.none, GameExternalMsg.NoOp)
