namespace BigTacToe.Server

open System
open BigTacToe.Shared

// https://joffreydalet.medium.com/f-back-end-and-thread-safe-data-collection-using-mailboxprocessor-7789839e1814
module GameManager =
    type private State =
        { OngoingGames: Map<GameId, GameModel> }

    type private Message =
        | StartGame
    
    type ThreadSafeData() =
        let agent =
            MailboxProcessor.Start (fun (inbox: MailboxProcessor<Message>) ->
                let rec loop(currentState: State) =
                    async {
                        let! msg = inbox.Receive()
                        match msg with
                        | StartGame ->
                            return! loop(currentState)
                    }
            
                loop({ State.OngoingGames = Map.empty })
            )
    
        member this.StartGame() =
            agent.Post StartGame // needs to reply gameId
    

