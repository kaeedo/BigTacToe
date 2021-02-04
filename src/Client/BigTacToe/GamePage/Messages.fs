namespace BigTacToe.Pages

open Fabulous
open SkiaSharp
open BigTacToe.Shared
open BigTacToe.Shared.SignalRHub
open Fable.SignalR.Elmish

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
        //| DisplayNewGameAlert ->
        //    let alertResult =
        //        async {
        //            let! confirmation = Application.Current.MainPage.DisplayAlert("New Game", "Are you sure you want to start a new game?", "Yes", "No") |> Async.AwaitTask
        //            return NewGameAlertResult confirmation
        //        }

        //    model, Cmd.ofAsyncMsg alertResult
        //| NewGameAlertResult shouldStartNew ->
        //    if shouldStartNew
        //    then GameModel.init (), Cmd.none
        //    else model, Cmd.none
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
                | _ -> Cmd.none // TODO: This

            { model with Hub = hub }, cmd, GameExternalMsg.NoOp
        | SignalRMessage response -> SignalRMessages.handleSignalRMessage model response
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
                let newGm = GameRules.updateModel gm sb
                { model with GameModel = newGm }, Cmd.none, GameExternalMsg.NoOp
            | None -> model, Cmd.none, GameExternalMsg.NoOp // TODO: FIX THIS

        | SKSurfaceTouched point when (gm.CurrentPlayer.PlayerId = model.MyStatus.PlayerId)
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
                let newGm = GameRules.updateModel gm subBoard

                let command =
                    match model.OpponentStatus with
                    | Joined _ -> Cmd.SignalR.send model.Hub (Action.MakeMove(model.GameId, gameMove))
                    | LocalAiGame -> Cmd.ofAsyncMsg <| AiPlayer.playPosition newGm
                    | _ -> Cmd.none

                ({ model with GameModel = newGm }, command, GameExternalMsg.NoOp)
        | GoToMainMenu ->
            // TODO: Send gameQuite message
            model, Cmd.none, GameExternalMsg.NavigateToMainMenu
        | _ -> (model, Cmd.none, GameExternalMsg.NoOp)
