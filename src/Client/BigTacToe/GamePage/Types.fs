namespace BigTacToe.Pages

open SkiaSharp
open BigTacToe.Shared

type ClientGameModel =
    { Size: float * float 
      GameModel: GameModel }

type GameMsg =
    //| DisplayNewGameAlert
    //| NewGameAlertResult of bool
    | ResizeCanvas of SKSizeI
    | SKSurfaceTouched of SKPoint
    | OpponentPlayed of PositionPlayed

type GameExternalMsg =
    | NoOp
    | NavigateToMainMenu