namespace BigTacToe.Pages

open System
open Fabulous
open SkiaSharp
open BigTacToe.Shared
open BigTacToe.Shared.SignalRHub
open Fable.SignalR.Elmish
open Xamarin.Forms

module internal Messages =
    let private calculateGlobalTileIndex (size: int * int) (point: SKPoint) =
        let (sizeX, sizeY) = size
        let (pointX, pointY) = point.X, point.Y

        let x =
            let segmentSize = float32 sizeX / 9.0f
            int (pointX / segmentSize)

        let y =
            let segmentSize = float32 sizeY / 9.0f
            int (pointY / segmentSize)

        x, y

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
                  Size = (smallerDimension, smallerDimension) },
            Cmd.none,
            GameExternalMsg.NoOp
        | OpponentPlayed positionPlayed ->
            let tileIndex =
                let (sbi, sbj) = fst positionPlayed
                let (ti, tj) = snd positionPlayed
                (ti + (sbi * 3)), (tj + (sbj * 3))

            let subBoards = GameRules.tryPlayPosition gm tileIndex

            match subBoards with
            | Some sb ->
                let gameMove =
                    { GameMove.Player = gm.CurrentPlayer
                      PositionPlayed = positionPlayed }

                let newGm = GameRules.updateModel gm sb gameMove

                let animations, commands =
                    Animations.getAnimations model newGm gameMove

                let model =
                    { model with
                          GameModel = newGm
                          Animations = animations }

                model, Cmd.batch commands, GameExternalMsg.NoOp
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
                                      && gm.Board.Winner.IsNone ->
            let globalTileIndex =
                calculateGlobalTileIndex model.Size point

            let positionPlayed =
                let (tileIndexI, tileIndexJ) = globalTileIndex

                let subBoardIndexI = tileIndexI / 3
                let subBoardIndexJ = tileIndexJ / 3
                (subBoardIndexI, subBoardIndexJ), (tileIndexI % 3, tileIndexJ % 3)

            let gameMove =
                { GameMove.Player = model.GameModel.CurrentPlayer
                  PositionPlayed = positionPlayed }

            match GameRules.tryPlayPosition gm globalTileIndex with
            | None -> (model, Cmd.none, GameExternalMsg.NoOp)
            | Some subBoard ->
                let newGm =
                    GameRules.updateModel gm subBoard gameMove

                let animations, commands =
                    Animations.getAnimations model newGm gameMove

                ({ model with
                       GameModel = newGm
                       Animations = animations },
                 Cmd.batch commands,
                 GameExternalMsg.NoOp)
        | SKSurfaceTouched point when (model.OpponentStatus <> LocalGame)
                                      && (gm.CurrentPlayer.PlayerId = model.MyStatus.PlayerId)
                                      && gm.Board.Winner.IsNone ->
            let globalTileIndex =
                calculateGlobalTileIndex model.Size point

            let positionPlayed =
                let (tileIndexI, tileIndexJ) = globalTileIndex

                let subBoardIndexI = tileIndexI / 3
                let subBoardIndexJ = tileIndexJ / 3
                (subBoardIndexI, subBoardIndexJ), (tileIndexI % 3, tileIndexJ % 3)

            let gameMove =
                { GameMove.Player = model.MyStatus
                  PositionPlayed = positionPlayed }

            match GameRules.tryPlayPosition gm globalTileIndex with
            | None -> (model, Cmd.none, GameExternalMsg.NoOp)
            | Some subBoard ->
                let newGm =
                    GameRules.updateModel gm subBoard gameMove

                let isGameOver = newGm.Board.Winner.IsSome

                let animations, commands =
                    Animations.getAnimations model newGm gameMove

                let command =
                    match model.OpponentStatus with
                    | Joined _ -> Cmd.SignalR.send model.Hub (Action.MakeMove(model.GameId, gameMove))
                    | LocalAiGame ->
                        if isGameOver
                        then Cmd.none
                        else Cmd.ofAsyncMsg <| AiPlayer.playPosition newGm
                    | _ -> Cmd.none

                let command = Cmd.batch (command :: commands)

                ({ model with
                       GameModel = newGm
                       Animations = animations },
                 command,
                 GameExternalMsg.NoOp)
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
