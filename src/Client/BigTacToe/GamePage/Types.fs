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

type ClientGameModel =
    { Size: int * int
      OpponentStatus: OpponentStatus
      StackLayout: ViewRef<StackLayout>
      GameId: int
      GameIdText: string
      MyStatus: Participant
      ShouldDrawLatestMove: bool
      AnimationPercent: float
      Hub: Elmish.Hub<Action, Response> option
      GameModel: GameModel }

type GameMsg =
    | ResizeCanvas of SKSizeI
    | SKSurfaceTouched of SKPoint
    | OpponentPlayed of PositionPlayed
    | ConnectToServer
    
    | ShouldDrawLatestMove of bool
    | SetAnimationPercent of float
    
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
