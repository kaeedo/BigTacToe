﻿namespace BigTacToe.Pages

open Fabulous
open SkiaSharp
open BigTacToe.Shared
open BigTacToe.Shared.SignalRHub
open Fable.SignalR.Elmish

module internal Messages =
    let private calculateTileIndex (size: int * int) (point: SKPoint) =
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
                    hub.WithUrl(sprintf "http://127.0.0.1:5000%s" Endpoints.Root)
                       .WithAutomaticReconnect()
                       .UseMessagePack()
                       .OnMessage SignalRMessage
                )
            model, cmd
        | RegisterHub hub ->
            let hub = Some hub

            let playerId = model.GameModel.CurrentPlayer.PlayerId

            let cmd =
                match model.OpponentStatus with
                | LookingForGame -> 
                    Cmd.batch 
                        [ Cmd.SignalR.send hub (Action.OnConnect playerId)
                          Cmd.SignalR.send hub (Action.SearchOrCreateGame playerId) ] 
                | _ -> Cmd.none // TODO: This

            { model with Hub = hub }, cmd
        | SignalRMessage response ->
            match response with
            | Response.GameStarted (gameId, participants) ->
                model, Cmd.none
            | _ -> model, Cmd.none // TODO: this
        | ResizeCanvas size ->
            let smallerDimension = if size.Width < size.Height then size.Width else size.Height
            //let board = { gm.Board with Board.Size = (smallerDimension, smallerDimension) }
            //let newGm = { gm with Board = board }
            { model with Size = (smallerDimension, smallerDimension) }, Cmd.none
        | OpponentPlayed positionPlayed ->
            let subBoards = GameRules.playPosition gm positionPlayed
            let newGm = GameRules.updateModel gm subBoards
            { model with GameModel = newGm }, Cmd.none

        // TODO: Figure out based on which meeple i'm supposed to be
        | SKSurfaceTouched point when (gm.CurrentPlayer.Meeple = Meeple.Ex) && gm.Board.Winner.IsNone -> 
            let tileIndex = calculateTileIndex model.Size point
            
            match GameRules.updatedBoard gm tileIndex with
            | None -> (model, Cmd.none)
            | Some subBoard ->
                let newGm = GameRules.updateModel gm subBoard

                let command =
                    if newGm.Board.Winner.IsSome 
                    then Cmd.none
                    else Cmd.ofAsyncMsg <| AiPlayer.playPosition newGm

                ({ model with GameModel = newGm }, command)
        | _ -> (model, Cmd.none)
            
                
