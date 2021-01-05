namespace BigTacToe.Server

open System
open BigTacToe.Shared
open System.Collections.Generic

module GameManager =
    let private getOtherMeeple participant =
        match participant with
        | Missing -> raise (BicTacToeException(InvalidPlayer))
        | Player (_, Meeple.Ex) -> Meeple.Oh
        | Player (_, Meeple.Oh) -> Meeple.Ex

    type private State =
        { OngoingGames: Map<GameId, GameModel> }

    type GameError =
    | NoOngoingGames
    | InvalidGameId
    | InvalidMove

    type private Message =
    | GetGame of GameId * AsyncReplyChannel<Result<GameId * GameModel, GameError>>
    | StartGame of Guid * AsyncReplyChannel<GameId>
    | TryJoinGame of Guid * AsyncReplyChannel<Result<GameId, GameError>>
    | JoinGame of Guid * GameId * AsyncReplyChannel<Result<GameId, GameError>>
    | PlayPosition of GameId * GameMove * AsyncReplyChannel<Result<GameModel * GameMove, GameError>>
    
    type Manager() =
        let rnd = Random()
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
                            let (gameId: GameId) =
                                let rec newId id =
                                    if state.OngoingGames.ContainsKey(id)
                                    then newId (rnd.Next(1000, 9999))
                                    else id
                                newId (rnd.Next(1000, 9999))
                            
                            rc.Reply gameId

                            let newGame = GameModel.init (Player (playerId, Meeple.Ex))
                            let ongoingGames = state.OngoingGames.Add (gameId, newGame)

                            return! loop { state with OngoingGames = ongoingGames }
                        | TryJoinGame (playerId, rc) ->
                            let availableGame =
                                state.OngoingGames
                                |> Seq.tryFind (fun kvp ->
                                    kvp.Value.Players
                                    |> snd
                                    |> function
                                        | Missing -> true
                                        | _ -> false
                                )

                            match availableGame with
                            | None -> 
                                rc.Reply (Result.Error NoOngoingGames)
                                return! loop state
                            | Some kvp ->
                                let player1 = fst kvp.Value.Players
                                let newPlayers = player1, Player (playerId, getOtherMeeple player1)
                                let newGm = { kvp.Value with Players = newPlayers }
                                let ongoingGames = state.OngoingGames.Add (kvp.Key, newGm)

                                rc.Reply (Result.Ok kvp.Key)

                                return! loop { state with OngoingGames = ongoingGames }
                        | JoinGame (playerId, gameId, rc) ->
                            let game = state.OngoingGames.TryFind gameId 
                            
                            match game with
                            | None -> 
                                rc.Reply (Result.Error InvalidGameId)
                                return! loop state
                            | Some g ->
                                let player1 = fst g.Players
                                let newPlayers = player1, Player (playerId, getOtherMeeple player1)
                                
                                let newGm = { g with Players = newPlayers }
                                let ongoingGames = state.OngoingGames.Add (gameId, newGm)
                                
                                rc.Reply (Result.Ok gameId)

                                return! loop { state with OngoingGames = ongoingGames }
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

                                    rc.Reply (Result.Ok (newGameModel, gameMove))

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
    
        member __.GetGame gameId =
            agent.PostAndReply (fun rc -> GetGame (gameId, rc))
        member __.StartGame playerId =
            agent.PostAndReply (fun rc -> StartGame (playerId, rc))
        member __.TryJoinGame playerId =
            agent.PostAndReply (fun rc -> TryJoinGame (playerId, rc))
        member __.JoinGame playerId gameId =
            agent.PostAndReply (fun rc -> JoinGame (playerId, gameId, rc))
        member __.PlayPosition gameId gameMove =
            agent.PostAndReply (fun rc -> PlayPosition (gameId, gameMove, rc))

