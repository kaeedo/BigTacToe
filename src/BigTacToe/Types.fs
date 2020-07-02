namespace BigTacToe

open SkiaSharp
open Xamarin.Forms
open Fabulous

type Meeple =
    | Empty = 0
    | Player = 1
    | Opponent = 2

type Tile =
    | Meeple of Meeple
    | SubBoard of Board

and Board =
    { Winner: Meeple
      Tiles: Tile [,] }

type Model =
  { TouchPoint: SKPoint
    CurrentPlayer: Meeple
    Board: Board
    StackLayout: ViewRef<StackLayout> }

type Msg =
    | SKSurfaceTouched of SKPoint

module Types =
    let private initBoard =
        let subBoard = Array2D.init 3 3 (fun _ _ -> Meeple <| Meeple.Empty)
        let bigBoard =
            { Board.Winner = Meeple.Empty
              Tiles = Array2D.init 3 3 (fun _ _ -> 
                { Board.Winner = Meeple.Empty
                  Tiles = subBoard }
                |> SubBoard
              ) 
            }

        bigBoard

    let initModel = 
        { Model.StackLayout = ViewRef<StackLayout>()
          CurrentPlayer = Meeple.Empty
          Board = initBoard
          TouchPoint = SKPoint.Empty }
