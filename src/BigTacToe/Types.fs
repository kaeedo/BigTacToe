namespace BigTacToe

open SkiaSharp
open Xamarin.Forms
open Fabulous
open System.Diagnostics

type MaybeBuilder() =
    member this.Bind(m, f) = Option.bind f m

    member this.Return(x) = Some x

    member this.ReturnFrom(x) = x

    member this.Zero() = None

    member this.Combine(a, b) =
        match a with
        | Some _ -> a
        | None -> b ()

    member this.Delay(f) = f

    member this.Run(f) = f ()

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

type Tile = SKRect * (Meeple option)

type SubBoard =
    { Winner: BoardWinner option
      Rect: SKRect
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
      Size: SKSizeI
      SubBoards: SubBoard [,] }
    with 
        member this.GetSubBoardIndex subBoard =
            let index =
                this.SubBoards
                |> Seq.cast<SubBoard>
                |> Seq.findIndex (fun sb -> sb = subBoard)
            index / 3, index % 3

type Model =
    { CurrentPlayer: Meeple
      Board: Board
      Size: float * float
      GridLayout: ViewRef<Grid> }

type PositionPlayed = (int * int) * (int * int)

type Msg =
    | ResizeCanvas of SKSizeI
    | SKSurfaceTouched of SKPoint
    | OpponentPlayed of PositionPlayed

module Types =
    let private initBoard =
        let subBoard =
            { SubBoard.Winner = None
              Rect = SKRect()
              IsPlayable = true
              Tiles = Array2D.init 3 3 (fun _ _ -> SKRect(), None) }

        let bigBoard =
            { Board.Winner = None
              Size = SKSizeI()
              SubBoards = Array2D.init 3 3 (fun _ _ -> subBoard) }

        bigBoard

    let initModel =
        { Model.GridLayout = ViewRef<Grid>()
          Size = 100.0, 100.0
          CurrentPlayer = Meeple.Ex
          Board = initBoard }
