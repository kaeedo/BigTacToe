namespace BigTacToe.Server

open System
open BigTacToe.Shared
open System.Collections.Generic

module GameManager =
    type private State =
        { OngoingGames: Map<GameId, GameModel>
          PendingGames: Map<GameId, GameModel * bool> }

    type GameError =
    | NoOngoingGames
    | InvalidGameId
    | InvalidMove

    type private Message =
    | GetGame of GameId * AsyncReplyChannel<Result<GameId * GameModel, GameError>>
    | StartGame of Guid * AsyncReplyChannel<GameId>
    | StartPrivateGame of Guid * AsyncReplyChannel<GameId>
    | JoinRandomGame of Guid * AsyncReplyChannel<Result<GameId, GameError>>
    | JoinPrivateGame of Guid * GameId * AsyncReplyChannel<Result<GameId, GameError>>
    | PlayPosition of GameId * GameMove * AsyncReplyChannel<Result<GameModel * GameMove, GameError>>
    
    let private rnd = Random()

    let private getOtherMeeple participant =
        match participant with
        | Missing -> raise (BicTacToeException(InvalidPlayer))
        | Player (_, Meeple.Ex) -> Meeple.Oh
        | Player (_, Meeple.Oh) -> Meeple.Ex

    let private idGenerator state =
        let rec newId id =
            if state.OngoingGames.ContainsKey(id)
            then newId (rnd.Next(1000, 9999))
            else id
        newId (rnd.Next(1000, 9999))
    
    type Manager() =
        let agent =
            MailboxProcessor.Start (fun (inbox: MailboxProcessor<Message>) ->
                let rec loop(state: State) =
                    async {
                        let! msg = inbox.Receive()
                        match msg with
                        | GetGame (gameId, rc) ->
                            match state.OngoingGames.TryFind gameId with
                            | Some g -> rc.Reply(Result.Ok (gameId, g))
                            | None -> rc.Reply(Result.Error InvalidGameId)

                            return! loop state
                        | StartGame (playerId, rc) ->
                            let (gameId: GameId) = idGenerator state
                            
                            rc.Reply gameId

                            let newGame = GameModel.init (Player (playerId, Meeple.Ex))
                            let pendingGames = state.PendingGames.Add (gameId, (newGame, false))

                            return! loop { state with PendingGames = pendingGames }
                        | StartPrivateGame (playerId, rc) ->
                            let (gameId: GameId) = idGenerator state
                            
                            rc.Reply gameId

                            let newGame = GameModel.init (Player (playerId, Meeple.Ex))
                            let pendingGames = state.PendingGames.Add (gameId, (newGame, true))

                            return! loop { state with PendingGames = pendingGames }
                        | JoinRandomGame (playerId, rc) ->
                            let availableGame =
                                state.PendingGames
                                |> Seq.tryFind (fun kvp ->
                                    let (game, isPrivate) = kvp.Value

                                    game.Players
                                    |> snd
                                    |> function
                                        | Missing -> true
                                        | _ -> false
                                    |> (&&) (not isPrivate)
                                )

                            match availableGame with
                            | None -> 
                                rc.Reply (Result.Error NoOngoingGames)
                                return! loop state
                            | Some kvp ->
                                let (game, _) = kvp.Value

                                let player1 = fst game.Players
                                let newPlayers = player1, Player (playerId, getOtherMeeple player1)
                                let newGm = { game with Players = newPlayers }
                                let ongoingGames = state.OngoingGames.Add (kvp.Key, newGm)
                                let pendingGames = state.PendingGames.Remove kvp.Key

                                rc.Reply (Result.Ok kvp.Key)

                                return! loop { state with OngoingGames = ongoingGames; PendingGames = pendingGames }
                        | JoinPrivateGame (playerId, gameId, rc) ->
                            let game = state.PendingGames.TryFind gameId 
                            
                            match game with
                            | None -> 
                                rc.Reply (Result.Error InvalidGameId)
                                return! loop state
                            | Some g ->
                                let (game, _) = game.Value

                                let player1 = fst game.Players
                                let newPlayers = player1, Player (playerId, getOtherMeeple player1)
                                
                                let newGm = { game with Players = newPlayers }
                                let ongoingGames = state.OngoingGames.Add (gameId, newGm)
                                let pendingGames = state.PendingGames.Remove gameId
                                
                                rc.Reply (Result.Ok gameId)

                                return! loop { state with OngoingGames = ongoingGames; PendingGames = pendingGames }
                        | PlayPosition (gameId, gameMove, rc) ->
                            let game = state.OngoingGames.TryFind gameId 

                            match game with
                            | None -> 
                                rc.Reply (Result.Error InvalidGameId)
                                return! loop state
                            | Some g ->
                                if g.CurrentPlayer <> gameMove.Player
                                then 
                                    rc.Reply (Result.Error InvalidMove)
                                else
                                    let subBoards = GameRules.playPosition g gameMove.PositionPlayed
                                    let newGameModel = GameRules.updateModel g subBoards
                                    printfn "%A" newGameModel

                                    rc.Reply (Result.Ok (newGameModel, gameMove))

                                    match newGameModel.Board.Winner with
                                    | Some w ->
                                        printfn "Somebody won?"
                                        let ongoingGames = state.OngoingGames.Remove gameId
                                    
                                        return! loop { state with OngoingGames = ongoingGames }
                                    | None ->
                                        let ongoingGames = state.OngoingGames.Add (gameId, newGameModel)
                                        printfn "%A" ongoingGames
                                        return! loop { state with OngoingGames = ongoingGames }
                    }
            
                loop({ State.OngoingGames = Map.empty; PendingGames = Map.empty })
            )
    
        member __.GetGame gameId =
            agent.PostAndReply (fun rc -> GetGame (gameId, rc))
        member __.StartGame playerId =
            agent.PostAndReply (fun rc -> StartGame (playerId, rc))
        member __.StartPrivateGame playerId =
            agent.PostAndReply (fun rc -> StartPrivateGame (playerId, rc))
        member __.JoinRandomGame playerId =
            agent.PostAndReply (fun rc -> JoinRandomGame (playerId, rc))
        member __.JoinPrivateGame playerId gameId =
            agent.PostAndReply (fun rc -> JoinPrivateGame (playerId, gameId, rc))
        member __.PlayPosition gameId gameMove =
            agent.PostAndReply (fun rc -> PlayPosition (gameId, gameMove, rc))

