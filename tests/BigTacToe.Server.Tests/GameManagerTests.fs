namespace BigTacToe.Server.Tests

open Expecto
open Swensen.Unquote
open BigTacToe.Server.GameManager
open System
open BigTacToe.Shared
open BigTacToe.Shared.Railways

module GameManagerTests =
    let private getGameInfo gameResult =
        match gameResult with
        | Result.Error e -> failtestf "Failed with: %s. Should have been able to get game" (e.ToString())
        | Result.Ok gameInfo -> gameInfo

    let private emptyBoardWithTwoPlayers (gm: Manager) player1 player2 =
        let gameId = gm.StartPrivateGame player1
        
        let joinResult = 
            (gm.JoinPrivateGame player2
            >=> gm.GetGame) gameId

        getGameInfo joinResult

    [<Tests>]
    let gameManagerTests = testList "Game Manager tests" [
        Tests.test "When no games exist, should not be able to get game" {
            // Arrange
            let gm = Manager()

            // Act
            let getGameResult = gm.GetGame 888

            // Assert
            test <@ getGameResult = Result.Error InvalidGameId @>
        }

        Tests.test "Should be able to start a game" {
            // Arrange
            let gm = Manager()
            let player1 = Guid.NewGuid()

            // Act
            let startedGameId = gm.StartGame player1

            // Assert
            test <@ startedGameId >= 1000 && startedGameId <= 9999 @>
        }

        Tests.test "Should be able to start a private game" {
            // Arrange
            let gm = Manager()
            let player1 = Guid.NewGuid()

            // Act
            let startedGameId = gm.StartPrivateGame player1

            // Assert
            test <@ startedGameId >= 1000 && startedGameId <= 9999 @>
        }

        Tests.test "Should be able to join a private game" {
            // Arrange
            let gm = Manager()
            let player1 = Guid.NewGuid()
            let player2 = Guid.NewGuid()

            let startedGameId = gm.StartPrivateGame player1

            // Act
            let joinPrivateGameResult = gm.JoinPrivateGame player2 startedGameId

            // Assert
            match joinPrivateGameResult with
            | Result.Ok gameId -> test <@ gameId = startedGameId @>
            | Result.Error e -> failtestf "Join Private Game should have succeeded"
        }

        Tests.test "Should be invalid game id when joining private non-existant game" {
            // Arrange
            let gm = Manager()

            // Act
            let joinPrivateGameResult = gm.JoinPrivateGame (Guid.NewGuid()) 8888

            // Assert
            test <@ joinPrivateGameResult = Result.Error InvalidGameId @>
        }

        Tests.test "When private game exists, should not be able to randomly join it" {
            // Arrange
            let gm = Manager()
            let player1 = Guid.NewGuid()
            let player2 = Guid.NewGuid()

            let _ = gm.StartPrivateGame player1

            // Act
            let joinGameIdResult = gm.JoinRandomGame (player2)

            // Assert
            test <@ joinGameIdResult = Result.Error NoOngoingGames @>
        }

        Tests.test "When one pending game exists, can only join one random game" {
            // Arrange
            let gm = Manager()
            let player1 = Guid.NewGuid()
            let player2 = Guid.NewGuid()

            let startedGameId = gm.StartGame player1
            
            let joinGameIdResult = gm.JoinRandomGame (player2)
            
            // Assume
            match joinGameIdResult with
            | Result.Ok joinedGameId -> test <@ startedGameId = joinedGameId @>
            | Result.Error e -> failtestf "Join Game should have succeeded"
            
            // Act
            let secondJoinGameResult = gm.JoinRandomGame (Guid.NewGuid())

            // Assert
            test <@ secondJoinGameResult = Result.Error NoOngoingGames @>
        }

        Tests.test "When a game is pending, should not be able to get game" {
            // Arrange
            let gm = Manager()
            let player1 = Guid.NewGuid()

            let startedGameId = gm.StartGame player1

            // Act
            let getGameResult = gm.GetGame startedGameId
            
            // Assert
            test <@ getGameResult = Result.Error InvalidGameId @>
        }

        Tests.test "When an ongoing game exists, should be able to get game" {
            // Arrange
            let gm = Manager()
            let player1 = Guid.NewGuid()
            let player2 = Guid.NewGuid()

            let startedGameId = gm.StartGame player1

            let joinGameIdResult = gm.JoinRandomGame (player2)

            // Assume
            match joinGameIdResult with
            | Result.Ok joinedGameId -> test <@ startedGameId = joinedGameId @>
            | Result.Error e -> failtestf "Join Game should have succeeded"

            // Act
            let getGameResult = gm.GetGame startedGameId

            // Assert
            match getGameResult with
            | Result.Ok (gameId, model) ->
                test <@ gameId = startedGameId @>
                test <@ model.Players = ((Participant.Player (player1, Meeple.Ex)), (Participant.Player (player2, Meeple.Oh))) @>
            | Result.Error e -> failtestf "Join Game should have succeeded"
        }

        testList "Play Position tests" [
            Tests.test "When playing position in a non-existant game, should be invalid game id" {
                // Arrange
                let gm = Manager()

                let gameMove = 
                    { GameMove.Player = Participant.Player (Guid.NewGuid(), Meeple.Ex)
                      PositionPlayed = (1, 1), (1, 1) }

                // Act
                let playPositionResult = gm.PlayPosition 8888 gameMove

                // Assert
                test <@ playPositionResult = Result.Error InvalidGameId @>
            }

            Tests.test "A new game should start with player1 who plays Ex" {
                // Arrange
                let gm = Manager()

                let player1 = Guid.NewGuid()
                let participant1 = Participant.Player (player1, Meeple.Ex)

                let player2 = Guid.NewGuid()
                
                let (_, gameModel) = emptyBoardWithTwoPlayers gm player1 player2

                // Assert
                test <@ gameModel.CurrentPlayer = participant1 @>
            }

            Tests.test "When player1 plays, current player should change" {
                // Arrange
                let gm = Manager()

                let player1 = Guid.NewGuid()
                let participant1 = Participant.Player (player1, Meeple.Ex)

                let player2 = Guid.NewGuid()
                let participant2 = Participant.Player (player2, Meeple.Oh)
                
                let (gameId, gameModel) = emptyBoardWithTwoPlayers gm player1 player2

                // Assume
                test <@ gameModel.CurrentPlayer = participant1 @>

                let positionPlayed = 
                    { GameMove.Player = participant1
                      PositionPlayed = (0, 1), (1, 1) }

                // Act
                let (gameModel, _) = gm.PlayPosition gameId positionPlayed |> getGameInfo

                // Assert
                test <@ gameModel.CurrentPlayer = participant2 @>
            }

            Tests.ftest "When player1 and player2 play valid moves, current player should be player1" {
                // Arrange
                let gm = Manager()

                let player1 = Guid.NewGuid()
                let participant1 = Participant.Player (player1, Meeple.Ex)

                let player2 = Guid.NewGuid()
                let participant2 = Participant.Player (player2, Meeple.Oh)
                
                let (gameId, _) = emptyBoardWithTwoPlayers gm player1 player2

                printfn "%A" gameId

                // Act1
                let positionPlayed = 
                    { GameMove.Player = participant1
                      PositionPlayed = (0, 1), (1, 1) }
                let (gameModel, _) = gm.PlayPosition gameId positionPlayed |> getGameInfo

                // Assert1
                test <@ gameModel.CurrentPlayer = participant2 @>
                test <@ gameModel.Board.SubBoards.[0, 1].Tiles.[1, 1] |> snd = Option.Some participant1 @>

                // Act2
                let positionPlayed = 
                    { GameMove.Player = participant2
                      PositionPlayed = (1, 1), (2, 1) }
                let playResult = gm.PlayPosition gameId positionPlayed

                // Assert2
                match playResult with
                | Result.Ok (gameModel, gameMove) -> test <@ gameModel.CurrentPlayer = participant1 @>
                | Result.Error e -> failtestf "Failed with: %s. Player 2 should have been able to play" (e.ToString())
            }
        ]
    ]