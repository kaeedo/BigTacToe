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
    | Quit

type ClientGameModel =
    { Size: int * int
      OpponentStatus: OpponentStatus
      GameId: int
      MyStatus: Participant
      Hub: Elmish.Hub<Action, Response> option
      GameModel: GameModel }

type GameMsg =
    | ResizeCanvas of SKSizeI
    | SKSurfaceTouched of SKPoint
    | OpponentPlayed of PositionPlayed
    | ConnectToServer

    | RegisterHub of Elmish.Hub<Action, Response>
    | SignalRMessage of Response

    | GoToMainMenu
    | DisplayGameQuitAlert
    | GameQuitAlertResult of bool
    
    | UnrecoverableError
    | ReturnToMainMenu

type GameExternalMsg =
    | NoOp
    | NavigateToMainMenu
