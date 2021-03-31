namespace BigTacToe.Pages

open Fabulous
open SkiaSharp
open BigTacToe.Shared
open BigTacToe.Shared.SignalRHub
open Fable.SignalR.Elmish
open Xamarin.Forms

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
    | Winner of BoardWinner

type DrawingAnimation =
    { Drawing: Drawing
      AnimationPercent: float32
      Animation: Animation }
    static member init drawing =
        { DrawingAnimation.Drawing = drawing
          AnimationPercent = 0.0f
          Animation = Animation() }
    
type AnimationMessage =
    | AddAnimation of DrawingAnimation
    | FinishAnimation of Drawing
    | AnimatePercent of Drawing * float32

type ClientGameModel =
    { Size: int
      OpponentStatus: OpponentStatus
      Canvas: ViewRef<Views.Forms.SKCanvasView>
      GameId: int
      GameIdText: string
      MyStatus: Participant
      RunningAnimation: DrawingAnimation option
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
