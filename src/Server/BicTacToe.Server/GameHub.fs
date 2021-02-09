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
            return (Response.GameFinished BoardWinner.Draw)
        }

    let private send (msg: Action) (hubContext: FableHub<Action, Response>) =
        let manager = hubContext.Services.GetService<GameManager.Manager>()

        match msg with
        | Action.OnConnect playerId ->
            printfn "%A has connected" playerId
            task {
                do! hubContext.Groups.AddToGroupAsync(hubContext.Context.ConnectionId, playerId.ToString())
                do! hubContext.Clients.Group(playerId.ToString()).Send(Response.Connected)    
            } :> Task
        | Action.SearchOrCreateGame playerId ->
            let tryGetGame = 
                manager.JoinRandomGame
                >=> manager.GetGame

            match tryGetGame playerId with
            | Ok (gameId, game) ->
                match game.Players with
                | TwoPlayers (player1, player2) ->
                    Task.WhenAll(hubContext.Clients.Group(player1.PlayerId.ToString()).Send(Response.GameStarted (gameId, (player1, player2))), 
                                 hubContext.Clients.Group(player2.PlayerId.ToString()).Send(Response.GameStarted (gameId, (player1, player2))))
                | _ -> Task.FromResult(()) :> Task // TODO: FIX THIS
            | Error NoOngoingGames -> 
                let newGameId = manager.StartGame playerId 
                printfn "started new game with id: {%i} for player %A" newGameId playerId
                hubContext.Clients.Group(playerId.ToString()).Send(Response.GameReady newGameId)
            | Error _ -> Task.FromResult(()) :> Task // TODO: FIX THIS
        | Action.MakeMove (gameId, gameMove) ->
            printfn "Received make move: %A" (gameId, gameMove)
            match manager.PlayPosition gameId gameMove with
            | Error e -> Task.FromResult(()) :> Task // TODO: FIX THIS
            | Ok (game, gameMove) ->
                match game.Players with
                | TwoPlayers (player1, player2) ->
                    task {
                        do! Task.WhenAll(hubContext.Clients.Group(player1.PlayerId.ToString()).Send(Response.MoveMade gameMove),
                                         hubContext.Clients.Group(player2.PlayerId.ToString()).Send(Response.MoveMade gameMove))
                        
                        match game.Board.Winner with
                        | Some w ->
                            do! Task.WhenAll(hubContext.Clients.Group(player1.PlayerId.ToString()).Send(Response.GameFinished w),
                                             hubContext.Clients.Group(player2.PlayerId.ToString()).Send(Response.GameFinished w))
                        | None -> ()
                    } :> Task
                        
                | _ -> Task.FromResult(()) :> Task // TODO: FIX THIS
        | Action.HostPrivateGame playerId ->
            // send waiting for opponent
            let newGame = manager.StartPrivateGame playerId
            hubContext.Clients.Group(playerId.ToString()).Send(Response.GameReady newGame)
        | Action.JoinPrivateGame (gameId, playerId) ->
            let tryJoinGame =
                manager.JoinPrivateGame playerId
                >=> manager.GetGame

            match tryJoinGame gameId with
            | Ok (gameId, game) ->
                match game.Players with
                | TwoPlayers (player1, player2) ->
                    Task.WhenAll(hubContext.Clients.Group(player1.PlayerId.ToString()).Send(Response.GameStarted (gameId, (player1, player2))), 
                                 hubContext.Clients.Group(player2.PlayerId.ToString()).Send(Response.GameStarted (gameId, (player1, player2))))
                | _ -> Task.FromResult(()) :> Task // TODO: FIX THIS
            | Error _ -> Task.FromResult(()) :> Task // TODO: FIX THIS
        | Action.QuitGame (gameId, playerId) ->
            let gameModelResult = manager.PlayerQuit gameId playerId
            
            match gameModelResult with
            | Ok gameModel ->
                let (OnePlayer remainingPlayer) = gameModel.Players
                hubContext.Clients.Group(remainingPlayer.PlayerId.ToString()).Send(Response.PlayerQuit)
            | Error _ -> Task.FromResult(()) :> Task // TODO: FIX THIS
            
        
    let private config =
        { SignalR.Config.Default<Action, Response>()
            with 
                UseMessagePack = true
                OnConnected = None
                OnDisconnected = None }

    let settings =
        { SignalR.Settings.EndpointPattern = Endpoints.Root
          SignalR.Settings.Send = send
          SignalR.Settings.Invoke = invoke 
          SignalR.Settings.Config = Some config }