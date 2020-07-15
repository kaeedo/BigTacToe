namespace BigTacToe

open SkiaSharp
open Xamarin.Forms
open Fabulous

type MaybeBuilder() =
    member this.Bind(value, fn) =
        match value with
        | None -> None
        | Some r -> fn r

    member this.Return value = Some value

type Meeple =
    | Ex = 0
    | Oh = 1

type Tile = SKRect * (Meeple option)

type SubBoard =
    { Winner: Meeple option
      Rect: SKRect
      IsPlayable: bool
      Tiles: Tile [,] }

type Board =
    { Winner: Meeple option
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
