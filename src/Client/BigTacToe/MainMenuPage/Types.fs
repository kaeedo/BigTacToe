namespace BigTacToe.Pages

type MainMenuModel = obj

type MainMenuMsg =
| NavigateToAiGame
| NavigateToHotSeatGame
| NavigateToMatchmakingGame
| NavigateToPrivateGame
| NavigateToHelp

type Opponent =
| Ai
| HotSeat
| Random
| Private

type MainMenuExternalMsg =
| NoOp
| NavigateToGame of Opponent
| NavigateToHelp