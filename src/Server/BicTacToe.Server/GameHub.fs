namespace BicTacToe.Server

open System
open Fable.SignalR
open BigTacToe.Shared
open BigTacToe.Shared.SignalRHub
open System.Collections.Generic
open FSharp.Control.Tasks.V2

module GameHub =
    let invoke (msg: Action) (hubContext: FableHub) =
        task {
            return (Response.GameFinished Meeple.Ex)
        }

    let send (msg: Action) (hubContext: FableHub<Action, Response>) =
        //let participants = hubContext.Services.GetService<Dictionary<string, string>>()

        match msg with
        | Action.SearchForGame playerId ->
            hubContext.Clients.All.Send (Response.GameFinished Meeple.Ex)
        | Action.MakeMove (gameId, playerId, positionPlayed) ->
            hubContext.Clients.All.Send (Response.GameFinished Meeple.Ex)
        | Action.HostGame playerId ->
            hubContext.Clients.All.Send (Response.GameFinished Meeple.Ex)
        | Action.JoinGame (gameId, playerId) ->
            hubContext.Clients.All.Send (Response.GameFinished Meeple.Ex)
        //| Action.ClientConnected participant -> 
        //    //participants.[participant] <- hubContext.Context.ConnectionId
        //    Response.ParticipantConnected (Seq.empty |> List.ofSeq)
        //    |> hubContext.Clients.All.Send
        //| Action.SendMessageToAll message -> 
        //    Response.ReceiveMessage message
        //    |> hubContext.Clients.All.Send
        //| Action.SendMessageToUser (recipient, message) -> 
        //    let sender = 
        //        Seq.empty
        //        |> List.ofSeq
        //        |> List.find (fun k -> 
        //            true
        //        )

        //    let recipientConnectionId = ""
        //    Response.ReceiveDirectMessage (sender, message)
        //    |> hubContext.Clients.Client(recipientConnectionId).Send

    let config =
        { SignalR.Settings.EndpointPattern = Endpoints.Root
          SignalR.Settings.Send = send
          SignalR.Settings.Invoke = invoke 
          SignalR.Settings.Config = None }