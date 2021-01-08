namespace BicTacToe.Server

open System
open Fable.SignalR
open BigTacToe.Shared
open BigTacToe.Shared.SignalRHub
open BigTacToe.Shared.Railways
open BigTacToe.Server.GameManager
open Microsoft.Extensions.DependencyInjection
open FSharp.Control.Tasks.V2
open BigTacToe.Server
open System.Threading.Tasks
open Fable.SignalR

module GameHub =
    let private invoke (msg: Action) (hubContext: FableHub) =
        task {
            return (Response.GameFinished Meeple.Ex)
        }

    let private send (msg: Action) (hubContext: FableHub<Action, Response>) =
        let manager = hubContext.Services.GetService<GameManager.Manager>()

        match msg with
        | Action.OnConnect playerId ->
            hubContext.Groups.AddToGroupAsync(hubContext.Context.ConnectionId, playerId.ToString())
        | Action.SearchOrCreateGame playerId ->
            let tryGetGame = 
                manager.JoinRandomGame
                >=> manager.GetGame

            match tryGetGame playerId with
            | Ok (gameId, game) ->
                let (player1, player2) = game.Players
                Task.WhenAll(hubContext.Clients.Group(player1.ToString()).Send(Response.GameStarted (gameId, Meeple.Ex)), 
                             hubContext.Clients.Group(player2.ToString()).Send(Response.GameStarted (gameId, Meeple.Ex)))
            | Error NoOngoingGames -> 
                let newGame = manager.StartGame playerId
                // send waiting for opponent
                hubContext.Clients.Group(playerId.ToString()).Send(Response.GameStarted (newGame, Meeple.Ex))
            | Error _ -> Task.FromResult(()) :> Task // FIX THIS

        | Action.MakeMove (gameId, gameMove) ->
            match manager.PlayPosition gameId gameMove with
            | Error e -> Task.FromResult(()) :> Task // FIX THIS
            | Ok (game, gameMove) ->
                let (player1, player2) = game.Players
                let moveMade = gameMove
                Task.WhenAll(hubContext.Clients.Group(player1.ToString()).Send(Response.MoveMade moveMade), 
                             hubContext.Clients.Group(player2.ToString()).Send(Response.MoveMade moveMade))

        | Action.HostPrivateGame playerId ->
            // send waiting for opponent
            let newGame = manager.StartGame playerId
            hubContext.Clients.Group(playerId.ToString()).Send(Response.GameStarted (newGame, Meeple.Ex))
        | Action.JoinPrivateGame (gameId, playerId) ->
            let tryJoinGame =
                manager.JoinPrivateGame playerId
                >=> manager.GetGame

            match tryJoinGame gameId with
            | Ok (gameId, gameModel) ->
                // TODO Handle missing player2
                let (player1, player2) = gameModel.Players
                Task.WhenAll(hubContext.Clients.Group(player1.ToString()).Send(Response.GameStarted (gameId, Meeple.Ex)), 
                             hubContext.Clients.Group(player2.ToString()).Send(Response.GameStarted (gameId, Meeple.Ex)))
            | Error _ -> Task.FromResult(()) :> Task // FIX THIS
        
    let private config =
        { SignalR.Config.Default<_, _>()
            with 
                UseMessagePack = true
                OnConnected = None
                OnDisconnected = None }

    let settings =
        { SignalR.Settings.EndpointPattern = Endpoints.Root
          SignalR.Settings.Send = send
          SignalR.Settings.Invoke = invoke 
          SignalR.Settings.Config = Some config }