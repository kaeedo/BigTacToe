namespace BigTacToe.Pages

open BigTacToe.Pages

type MainMenuModel = obj

type Opponent =
| Ai
| HotSeat
| Random
| Private

type MainMenuMsg =
| NavigateToAiGame
| NavigateToHotSeatGame

| CheckServerResponse of string * Opponent
| CheckServerFailed

| NavigateToOnlineGame of Opponent
| NavigateToHelp
| CheckServer of Opponent

type MainMenuExternalMsg =
| NoOp
| NavigateToGame of Opponent
| NavigateToHelp