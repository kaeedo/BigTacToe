﻿namespace BigTacToe.Shared

open System

type Meeple =
    | Ex
    | Oh
    with override this.ToString() =
            match this with
            | Ex -> "X"
            | Oh -> "O"

type BoardWinner =
    | Player of Meeple
    | Draw


type Rect = float32 * float32 * float32 * float32 // left, top, right, bottom

type Point = float32 * float32 // x, y

type Tile = Rect * (Meeple option)

type SubBoard =
    { Winner: BoardWinner option
      Rect: Rect
      IsPlayable: bool
      Tiles: Tile [,] }
    with 
        member this.GetTileIndex tile =
            let index =
                this.Tiles
                |> Seq.cast<Tile>
                |> Seq.findIndex (fun t -> t = tile)
            index / 3, index % 3

type Board =
    { Winner: BoardWinner option
      Size: int * int
      SubBoards: SubBoard [,] }
    with 
        member this.GetSubBoardIndex subBoard =
            let index =
                this.SubBoards
                |> Seq.cast<SubBoard>
                |> Seq.findIndex (fun sb -> sb = subBoard)
            index / 3, index % 3

type GameModel =
    { CurrentPlayer: Meeple
      Board: Board
      Size: float * float }
    with
      static member init () =
          let initBoard =
              let subBoard =
                  { SubBoard.Winner = None
                    Rect = 0.0f, 0.0f, 0.0f, 0.0f
                    IsPlayable = true
                    Tiles = Array2D.init 3 3 (fun _ _ -> (0.0f, 0.0f, 0.0f, 0.0f), None) }

              let bigBoard =
                  { Board.Winner = None
                    Size = (0, 0)
                    SubBoards = Array2D.init 3 3 (fun _ _ -> subBoard) }

              bigBoard

          { GameModel.Size = 100.0, 100.0 //GridLayout = ViewRef<Grid>()
            CurrentPlayer = Meeple.Ex
            Board = initBoard }

type PositionPlayed = (int * int) * (int * int)

// Game metadata

type GameId = int

module SignalRHub =
    [<RequireQualifiedAccess>]
    type Action =
    | SearchForGame of Guid
    | HostGame of Guid
    | JoinGame of GameId * Guid
    | MakeMove of GameId * Guid * PositionPlayed

    [<RequireQualifiedAccess>]
    type Response =
    | GameStarted of GameId * Meeple
    | GameReady of GameId
    | MoveMade of Meeple * PositionPlayed // Maybe Result<_, isValid: bool>
    | GameFinished of Meeple

[<RequireQualifiedAccess>]
module Endpoints =
    let [<Literal>] Root = "/gamehub"