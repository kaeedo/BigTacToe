﻿namespace BigTacToe.Shared

open System

type BigTacToeExceptionMessage =
| InvalidPlayer

type BicTacToeException(msg: BigTacToeExceptionMessage) =
    inherit Exception(msg.ToString())

type Meeple =
| Ex
| Oh
    with override this.ToString() =
            match this with
            | Ex -> "X"
            | Oh -> "O"

type Participant = 
| Player of Guid * Meeple
| Missing
    with override this.ToString() =
            match this with
            | Player (id, _) -> id.ToString()
            | Missing -> "Player missing"

type BoardWinner =
| Participant of Participant
| Draw

type Tile = (int * int) * (Participant option)

type SubBoard =
    { Winner: BoardWinner option
      Index: int * int
      IsPlayable: bool
      Tiles: Tile [,] }

type Board =
    { Winner: BoardWinner option
      SubBoards: SubBoard [,] }

type GameModel =
    { Players: Participant * Participant
      CurrentPlayer: Participant
      Board: Board }
    with
      static member init participant =
          let initBoard =
              let subBoard i j =
                  let newI = i + (i * 2)
                  let newJ = j + (j * 2)
                  { SubBoard.Winner = None
                    Index = i, j
                    IsPlayable = true
                    Tiles = Array2D.init 3 3 (fun i j -> (newI + i, newJ + j), None) }

              let bigBoard =
                  { Board.Winner = None
                    SubBoards = Array2D.init 3 3 (fun i j -> subBoard i j) }

              bigBoard

          { GameModel.Players = (participant, Participant.Missing)
            CurrentPlayer = participant
            Board = initBoard }

type PositionPlayed = (int * int) * (int * int)

type GameMove =
    { Player: Participant
      PositionPlayed: PositionPlayed }

// Game metadata

type GameId = int

module SignalRHub =
    [<RequireQualifiedAccess>]
    type Action =
    | OnConnect of Guid
    | SearchOrCreateGame of Guid
    | HostPrivateGame of Guid
    | JoinPrivateGame of GameId * Guid
    | MakeMove of GameId * GameMove

    [<RequireQualifiedAccess>]
    type Response =
    | GameStarted of GameId * Meeple
    | GameReady of GameId
    | MoveMade of GameMove // Maybe Result<_, isValid: bool>
    | GameFinished of Meeple

[<RequireQualifiedAccess>]
module Endpoints =
    let [<Literal>] Root = "/gamehub"