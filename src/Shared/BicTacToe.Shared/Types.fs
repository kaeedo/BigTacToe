namespace BigTacToe.Shared

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

type Rect = float32 * float32 * float32 * float32 // left, top, right, bottom

type Point = float32 * float32 // x, y

type Tile = Rect * (Participant option)

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
    { Players: Participant * Participant
      CurrentPlayer: Participant
      Board: Board }
    with
      static member init participant =
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
    | SearchForGame of Guid
    | HostGame of Guid
    | JoinGame of GameId * Guid
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