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
                maybe {
                    let! player1 = game.Player1
                    let! player2 = game.Player2

                    return
                        Task.WhenAll(hubContext.Clients.Group(player1.PlayerId.ToString()).Send(Response.GameStarted (gameId, (player1, player2))), 
                                        hubContext.Clients.Group(player2.PlayerId.ToString()).Send(Response.GameStarted (gameId, (player1, player2))))
                }
                |> function
                | Some t -> t
                | None -> Task.FromResult(()) :> Task // TODO: FIX THIS
            | Error NoOngoingGames -> 
                let newGameId = manager.StartGame playerId
                hubContext.Clients.Group(playerId.ToString()).Send(Response.GameReady newGameId)
            | Error _ -> Task.FromResult(()) :> Task // TODO: FIX THIS
            

        | Action.MakeMove (gameId, gameMove) ->
            match manager.PlayPosition gameId gameMove with
            | Error e -> Task.FromResult(()) :> Task // TODO: FIX THIS
            | Ok (game, gameMove) ->
                maybe {
                    let! player1 = game.Player1
                    let! player2 = game.Player2

                    return 
                        Task.WhenAll(hubContext.Clients.Group(player1.PlayerId.ToString()).Send(Response.MoveMade gameMove), 
                                     hubContext.Clients.Group(player2.PlayerId.ToString()).Send(Response.MoveMade gameMove))
                }
                |> function
                | Some t -> t
                | None -> Task.FromResult(()) :> Task // TODO: FIX THIS

        | Action.HostPrivateGame playerId ->
            // send waiting for opponent
            let newGame = manager.StartGame playerId
            hubContext.Clients.Group(playerId.ToString()).Send(Response.GameReady newGame)
        | Action.JoinPrivateGame (gameId, playerId) ->
            let tryJoinGame =
                manager.JoinPrivateGame playerId
                >=> manager.GetGame

            match tryJoinGame gameId with
            | Ok (gameId, game) ->
                maybe {
                    let! player1 = game.Player1
                    let! player2 = game.Player2

                    return 
                        Task.WhenAll(hubContext.Clients.Group(player1.ToString()).Send(Response.GameStarted (gameId, (player1, player2))), 
                                     hubContext.Clients.Group(player2.ToString()).Send(Response.GameStarted (gameId, (player1, player2))))
                }
                |> function
                | Some t -> t
                | None -> Task.FromResult(()) :> Task // TODO: FIX THIS
            | Error _ -> Task.FromResult(()) :> Task // TODO: FIX THIS
        
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