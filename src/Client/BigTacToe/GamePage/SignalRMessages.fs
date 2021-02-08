namespace BigTacToe.Pages

open Fabulous
open BigTacToe.Shared
open BigTacToe.Shared.SignalRHub
open Fable.SignalR.Elmish

module internal SignalRMessages =
    let handleSignalRMessage model response =
        match response with
        | Response.Connected ->
            model,
            Cmd.SignalR.send model.Hub (Action.SearchOrCreateGame model.GameModel.CurrentPlayer.PlayerId),
            GameExternalMsg.NoOp
        | Response.GameStarted (gameId, participants) ->
            let me =
                if (fst participants).PlayerId = model.MyStatus.PlayerId
                then fst participants
                else snd participants

            let opponent =
                if (fst participants).PlayerId = model.MyStatus.PlayerId
                then snd participants
                else fst participants

            let gameModel =
                { model.GameModel with
                      Players = TwoPlayers(me, opponent)
                      CurrentPlayer = fst participants }

            { model with
                  GameId = gameId
                  OpponentStatus = Joined opponent
                  MyStatus = me
                  GameModel = gameModel },
            Cmd.none,
            GameExternalMsg.NoOp
        | Response.MoveMade gm ->
            if gm.Player.PlayerId = model.MyStatus.PlayerId
            then model, Cmd.none, GameExternalMsg.NoOp
            else model, (Cmd.ofMsg (OpponentPlayed gm.PositionPlayed)), GameExternalMsg.NoOp
        | Response.GameFinished w ->
            model, Cmd.none, GameExternalMsg.NoOp
        | Response.PlayerQuit ->
            let board = { model.GameModel.Board with Winner = Some (Participant model.MyStatus) }
            let gm =
                { model.GameModel with
                    Players = OnePlayer model.MyStatus
                    Board = board }
            { model with GameModel = gm; OpponentStatus = Quit }, Cmd.none, GameExternalMsg.NoOp
        // | _ -> model, Cmd.none, GameExternalMsg.NoOp // TODO: this
