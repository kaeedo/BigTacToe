namespace BigTacToe.Pages

open Fabulous
open SkiaSharp
open BigTacToe.Shared

module internal Messages =
    let private calculateTileIndex (size: float * float) (point: SKPoint) =
        let (sizeX, sizeY) = size
        let (pointX, pointY) = point.X, point.Y

        let x = 
            let segmentSize = float32 sizeX / 9.0f
            int (pointX / segmentSize)

        let y = 
            let segmentSize = float32 sizeY / 9.0f
            int (pointY / segmentSize)

        x, y

    let private isMe (currentPlayer: Participant) =
        match currentPlayer with
        | Player (_, m) -> m = Meeple.Ex
        | _ -> false

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
        | ResizeCanvas size ->
            let board = { gm.Board with Board.Size = (size.Width, size.Height) }
                //setBigSize gm.Board size.Width size.Height
            let newGm = { gm with Board = board }
            { model with GameModel = newGm }, Cmd.none
        | OpponentPlayed positionPlayed ->
            let subBoards = GameRules.playPosition gm positionPlayed
            let newGm = GameRules.updateModel gm subBoards
            { model with GameModel = newGm }, Cmd.none
        | SKSurfaceTouched point when (isMe gm.CurrentPlayer) && gm.Board.Winner.IsNone -> 
            let tileIndex = calculateTileIndex model.Size point
            
            match GameRules.updatedBoard gm tileIndex with
            | None -> (model, Cmd.none)
            | Some subBoard ->
                let newGm = GameRules.updateModel gm subBoard

                let command =
                    if newGm.Board.Winner.IsSome 
                    then Cmd.none
                    else Cmd.ofAsyncMsg <| CpuPlayer.playPosition newGm

                ({ model with GameModel = newGm }, command)
        | _ -> (model, Cmd.none)
            
                
