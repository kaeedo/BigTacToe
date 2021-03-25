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

type Drawing =
    | GameMove of GameMove
    | SubBoardWinner of SubBoard
    | Winner of Meeple
    
type AnimationMessage =
    | RemoveAnimation
    | AnimatePercent of Drawing * float

type DrawingAnimation =
    { Drawing: Drawing
      AnimationPercent: float }

type ClientGameModel =
    { Size: int * int
      OpponentStatus: OpponentStatus
      Canvas: ViewRef<Views.Forms.SKCanvasView>
      GameId: int
      GameIdText: string
      MyStatus: Participant
      Animations: DrawingAnimation list
      Hub: Elmish.Hub<Action, Response> option
      GameModel: GameModel }

type GameMsg =
    | ResizeCanvas of SKSizeI
    | SKSurfaceTouched of SKPoint
    | OpponentPlayed of PositionPlayed
    | ConnectToServer

    | AnimationMessage of AnimationMessage

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
