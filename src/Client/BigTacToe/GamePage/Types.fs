namespace BigTacToe.Pages

open SkiaSharp
open BigTacToe.Shared
open BigTacToe.Shared.SignalRHub
open Fable.SignalR.Elmish

type OpponentStatus =
    | LocalGame
    | LocalAiGame
    | LookingForGame
    | WaitingForPrivate of int
    | Joined of Participant

type ClientGameModel =
    { Size: int * int
      OpponentStatus: OpponentStatus
      Hub: Elmish.Hub<Action, Response> option
      GameModel: GameModel }

type GameMsg =
    //| DisplayNewGameAlert
    //| NewGameAlertResult of bool
    | ResizeCanvas of SKSizeI
    | SKSurfaceTouched of SKPoint
    | OpponentPlayed of PositionPlayed
    | ConnectToServer

    | RegisterHub of Elmish.Hub<Action, Response>
    | SignalRMessage of Response

type GameExternalMsg =
    | NoOp
    | NavigateToMainMenu