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

module GameHub =
    let private sendMessage (hubContext: FableHub<Action, Response>) (playerId: Guid) (message: SignalRHub.Response) =
        hubContext.Clients.Group(playerId.ToString()).Send(message)
        
    let private invoke (msg: Action) (hubContext: FableHub) =
        let manager =
            hubContext.Services.GetService<GameManager.Manager>()
            
        match msg with
        | Action.OnConnect playerId ->
            task {
               do! hubContext.Groups.AddToGroupAsync(hubContext.Context.ConnectionId, playerId.ToString())
               return Response.Connected
            }
        | Action.HostPrivateGame playerId ->
            task {
                let newGameId = manager.StartPrivateGame playerId
                return Response.PrivateGameReady newGameId
            }
        | _ -> task { return (Response.GameFinished BoardWinner.Draw) }

    let private send (msg: Action) (hubContext: FableHub<Action, Response>) =
        let manager =
            hubContext.Services.GetService<GameManager.Manager>()
            
        let sendMessage = sendMessage hubContext

        match msg with
        | Action.SearchOrCreateGame playerId ->
            let tryGetGame =
                manager.JoinRandomGame >=> manager.GetGame

            match tryGetGame playerId with
            | Ok (gameId, game) ->
                match game.Players with
                | TwoPlayers (p1, p2) ->
                    Task.WhenAll(sendMessage (p1.PlayerId) (Response.GameStarted(gameId, (p1, p2))),
                                 sendMessage (p2.PlayerId) (Response.GameStarted(gameId, (p1, p2))))
                | OnePlayer p -> sendMessage p.PlayerId Response.UnrecoverableError
                | NoOne -> Task.FromResult(()) :> Task
            | Error NoOngoingGames ->
                let newGameId = manager.StartGame playerId
                //printfn "started new game with id: {%i} for player %A" newGameId playerId
                Task.FromResult(()) :> Task
            | Error _ -> Task.FromResult(()) :> Task // TODO: FIX THIS
        | Action.MakeMove (gameId, gameMove) ->
            match manager.PlayPosition gameId gameMove with
            | Error e -> Task.FromResult(()) :> Task
                (*match e with
                | InvalidGameId -> sendMessage p.PlayerId Response.UnrecoverableError
                | InvalidMove -> sendMessage p.PlayerId Response.UnrecoverableError
                | _ -> sendMessage p.PlayerId Response.UnrecoverableError*)
            | Ok (game, gameMove) ->
                match game.Players with
                | TwoPlayers (player1, player2) ->
                    task {
                        do! Task.WhenAll
                                (sendMessage player1.PlayerId (Response.MoveMade gameMove),
                                 sendMessage player2.PlayerId (Response.MoveMade gameMove))

                        match game.Board.Winner with
                        | Some w ->
                            do! Task.WhenAll
                                    (sendMessage player1.PlayerId (Response.GameFinished w),
                                     sendMessage player2.PlayerId (Response.GameFinished w))
                        | None -> ()
                    } :> Task
                | OnePlayer p ->
                    sendMessage p.PlayerId Response.UnrecoverableError
                | NoOne -> Task.FromResult(()) :> Task
        | Action.JoinPrivateGame (gameId, playerId) ->
            // TODO: move to invoke
            let tryJoinGame =
                manager.JoinPrivateGame playerId
                >=> manager.GetGame

            match tryJoinGame gameId with
            | Ok (gameId, game) ->
                match game.Players with
                | TwoPlayers (p1, p2) ->
                    Task.WhenAll
                        (sendMessage p1.PlayerId (Response.GameStarted(gameId, (p1, p2))),
                         sendMessage p2.PlayerId (Response.GameStarted(gameId, (p1, p2))))
                | OnePlayer p ->
                    sendMessage p.PlayerId Response.UnrecoverableError
                | NoOne -> Task.FromResult(()) :> Task
            | Error _ -> Task.FromResult(()) :> Task // TODO: FIX THIS
        | Action.QuitGame (gameId, playerId) ->
            let gameModelResult = manager.PlayerQuit gameId playerId

            match gameModelResult with
            | Ok gameModel ->
                match gameModel.Players with
                |OnePlayer remainingPlayer ->
                    sendMessage remainingPlayer.PlayerId Response.PlayerQuit
                | TwoPlayers (p1, p2) ->
                    Task.WhenAll(sendMessage p1.PlayerId Response.UnrecoverableError,
                                 sendMessage p2.PlayerId Response.UnrecoverableError)
                | NoOne -> Task.FromResult(()) :> Task
            | Error _ -> Task.FromResult(()) :> Task

    let private config =
        { SignalR.Config.Default<Action, Response>() with
              UseMessagePack = true
              OnConnected = None
              OnDisconnected = None }

    let settings =
        { SignalR.Settings.EndpointPattern = Endpoints.Root
          SignalR.Settings.Send = send
          SignalR.Settings.Invoke = invoke
          SignalR.Settings.Config = Some config }
