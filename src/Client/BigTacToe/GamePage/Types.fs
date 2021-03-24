namespace BigTacToe.Pages

open Fabulous
open SkiaSharp
open BigTacToe.Shared
open BigTacToe.Shared.SignalRHub
open Fable.SignalR.Elmish

type OpponentStatus =
    | LocalGame
    | LocalAiGame
    | LookingForGame
    | WaitingForPrivate of GameId option
    | Joined of Participant
    | Quit

type AnimatingMeeple =
    { GameMove: GameMove
      AnimationPercent: float }

type ClientGameModel =
    { Size: int * int
      OpponentStatus: OpponentStatus
      Canvas: ViewRef<Views.Forms.SKCanvasView>
      GameId: int
      GameIdText: string
      MyStatus: Participant
      AnimatingMeeples: AnimatingMeeple list
      Hub: Elmish.Hub<Action, Response> option
      GameModel: GameModel }

type GameMsg =
    | ResizeCanvas of SKSizeI
    | SKSurfaceTouched of SKPoint
    | OpponentPlayed of PositionPlayed
    | ConnectToServer

    | RemoveAnimatingMeeple
    | AnimatePercent of GameMove * float

    | StartPrivateGame
    | JoinPrivateGame of string
    | EnterGameId of string

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
