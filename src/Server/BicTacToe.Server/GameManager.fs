namespace BigTacToe.Server

open System
open BigTacToe.Shared
open System.Collections.Generic

module GameManager =
    type private State =
        { OngoingGames: Map<GameId, GameModel> }

    type GameError =
    | NoOngoingGames
    | InvalidGameId
    | InvalidMove

    type private Message =
        | StartGame of Guid * AsyncReplyChannel<GameId>
        | TryJoinGame of Guid * AsyncReplyChannel<Result<GameId, GameError>>
        | PlayPosition of GameId * GameMove * AsyncReplyChannel<Result<PositionPlayed, GameError>>
    
    type Manager() =
        let rnd = Random()
        let agent =
            MailboxProcessor.Start (fun (inbox: MailboxProcessor<Message>) ->
                let rec loop(state: State) =
                    async {
                        let! msg = inbox.Receive()
                        match msg with
                        | StartGame (playerId, rc) ->
                            let (gameId: GameId) =
                                let rec newId id =
                                    if state.OngoingGames.ContainsKey(id)
                                    then newId (rnd.Next(1000, 9999))
                                    else id
                                newId (rnd.Next(1000, 9999))
                            
                            rc.Reply gameId

                            let newGame = GameModel.init playerId
                            let ongoingGames = state.OngoingGames.Add (gameId, newGame)

                            return! loop { state with OngoingGames = ongoingGames }
                        | TryJoinGame (playerId, rc) ->
                            let availableGame =
                                state.OngoingGames//.Keys
                                |> Seq.tryFind (fun kvp ->
                                    kvp.Value.Players
                                    |> snd
                                    |> Option.isNone
                                )

                            match availableGame with
                            | None -> 
                                rc.Reply (Result.Error NoOngoingGames)
                                return! loop state
                            | Some kvp ->
                                let newPlayers = (fst kvp.Value.Players), Some playerId
                                let newGm = { kvp.Value with Players = newPlayers }
                                let ongoingGames = state.OngoingGames.Add (kvp.Key, newGm)

                                rc.Reply (Result.Ok kvp.Key)

                                return! loop { state with OngoingGames = ongoingGames }
                        | PlayPosition (gameId, gameMove, rc) ->
                            let game = 
                                try
                                    Some state.OngoingGames.[gameId]
                                with
                                | :? KeyNotFoundException ->
                                    None

                            match game with
                            | None -> 
                                rc.Reply (Result.Error InvalidGameId)
                                return! loop state
                            | Some g ->
                                if g.CurrentPlayer <> gameMove.Meeple
                                then 
                                    rc.Reply (Result.Error InvalidMove)
                                else
                                    let subBoards = GameRules.playPosition g gameMove.PositionPlayed
                                    let newGameModel = GameRules.updateModel g subBoards

                                    rc.Reply (Result.Ok gameMove.PositionPlayed)

                                    match newGameModel.Board.Winner with
                                    | Some w ->
                                        let ongoingGames = state.OngoingGames.Remove gameId
                                    
                                        return! loop { state with OngoingGames = ongoingGames }
                                    | None ->
                                        let ongoingGames = state.OngoingGames.Add (gameId, newGameModel)
                                
                                        return! loop { state with OngoingGames = ongoingGames }
                    }
            
                loop({ State.OngoingGames = Map.empty })
            )
    
        member __.StartGame playerId =
            agent.PostAndAsyncReply (fun rc -> StartGame (playerId, rc))
        member __.TryJoinGame playerId =
            agent.PostAndAsyncReply (fun rc -> TryJoinGame (playerId, rc))
        member __.PlayPosition gameId gameMove =
            agent.PostAndAsyncReply (fun rc -> PlayPosition (gameId, gameMove, rc))

