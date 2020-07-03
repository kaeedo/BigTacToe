﻿namespace BigTacToe

open SkiaSharp
open Xamarin.Forms
open Fabulous

type Meeple =
    | Player = 0
    | Opponent = 1

type SubBoard =
    { Winner: Meeple option
      Rect: SKRect
      Tiles: (SKRect * (Meeple option)) [,] }

type Board =
    { Winner: Meeple option
      Size: SKSizeI
      SubBoards: SubBoard [,] }

type Model =
  { CurrentPlayer: Meeple option
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
              Tiles = Array2D.init 3 3 (fun _ _ -> SKRect(), None) }

        let bigBoard =
            { Board.Winner = None
              Size = SKSizeI()
              SubBoards = Array2D.init 3 3 (fun _ _ -> subBoard) }

        bigBoard

    let initModel = 
        { Model.StackLayout = ViewRef<StackLayout>()
          CurrentPlayer = None
          TouchPoint = SKPoint()
          Board = initBoard }
