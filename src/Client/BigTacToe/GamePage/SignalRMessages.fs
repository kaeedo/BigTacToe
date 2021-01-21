namespace BigTacToe.Pages

open Fabulous
open BigTacToe.Shared
open BigTacToe.Shared.SignalRHub
open Fable.SignalR.Elmish

module internal SignalRMessages =
    let handleSignalRMessage model response =
        printfn "Response received: %A" response
        match response with
        | Response.Connected ->
            model, Cmd.SignalR.send model.Hub (Action.SearchOrCreateGame model.GameModel.CurrentPlayer.PlayerId)
        | Response.GameStarted (gameId, participants) ->
            let me =
                if (fst participants).PlayerId = model.MyStatus.PlayerId
                then fst participants
                else snd participants
                
            let opponent =
                if (fst participants).PlayerId = model.MyStatus.PlayerId
                then snd participants
                else fst participants
            
            { model with OpponentStatus = Joined opponent; MyStatus = me }, Cmd.none
        | _ -> model, Cmd.none // TODO: this