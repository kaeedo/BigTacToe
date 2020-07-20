namespace BigTacToe

open SkiaSharp
open Xamarin.Forms
open Fabulous

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

type Board =
    { Winner: BoardWinner option
      Size: SKSizeI
      SubBoards: SubBoard [,] }

type Model =
    { CurrentPlayer: Meeple
      Board: Board
      TouchPoint: SKPoint
      StackLayout: ViewRef<StackLayout> }

type Msg =
    | ResizeCanvas of SKSizeI
    | SKSurfaceTouched of SKPoint

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
        { Model.StackLayout = ViewRef<StackLayout>()
          CurrentPlayer = Meeple.Ex
          TouchPoint = SKPoint(-1.0f, -1.0f)
          Board = initBoard }
