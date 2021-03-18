namespace BigTacToe.Pages

open Fabulous.XamarinForms
open Xamarin.Forms

open Fabulous

type MainMenuModel =
    { Count: int }

type Opponent =
| Ai
| HotSeat
| Random
| Private

type MainMenuMsg =
| NavigateToAiGame
| NavigateToHotSeatGame
| NavigateToMatchmakingGame
| NavigateToPrivateGame

type MainMenuExternalMsg =
| NoOp
| NavigateToGame of Opponent

[<RequireQualifiedAccess>]
module internal MainMenu =
    let init () = { MainMenuModel.Count = 0 }, Cmd.none

    let update message model =
        match message with
        | MainMenuMsg.NavigateToAiGame -> model, Cmd.none, MainMenuExternalMsg.NavigateToGame Ai
        | MainMenuMsg.NavigateToHotSeatGame -> model, Cmd.none, MainMenuExternalMsg.NavigateToGame HotSeat
        | MainMenuMsg.NavigateToMatchmakingGame -> model, Cmd.none, MainMenuExternalMsg.NavigateToGame Random
        | MainMenuMsg.NavigateToPrivateGame -> model, Cmd.none, MainMenuExternalMsg.NavigateToGame Private

    let view model dispatch =
        View.ContentPage
            (content =
                View.Grid
                    (rowdefs = [Absolute 50.0; Star; Absolute 50.0; Absolute 50.0],
                     coldefs = [Star; Star],
                     padding = Thickness 20.0,
                     //ref = ViewRef<Stack>(), //model.GridLayout,
                     children = [
                        View.Label(
                            text = "Big Tac Toe", 
                            fontSize = FontSize.Size 36.0, 
                            horizontalTextAlignment = TextAlignment.Center
                        ).ColumnSpan(2)
                        View.Image(
                            //source = Image.fromPath "placeholder.png",
                            aspect = Aspect.AspectFit
                        ).Row(1).ColumnSpan(2)
                        View.Button(
                            text = "Play vs. CPU",
                            command = (fun () -> dispatch MainMenuMsg.NavigateToAiGame),
                            cornerRadius = 10
                        ).Row(2)
                        View.Button(
                            text = "Pass the phone",
                            command = (fun () -> dispatch MainMenuMsg.NavigateToHotSeatGame),
                            cornerRadius = 10
                        ).Row(2).Column(1)
                        View.Button(
                            text = "Find opponent",
                            command = (fun () -> dispatch MainMenuMsg.NavigateToMatchmakingGame),
                            cornerRadius = 10
                        ).Row(3).Column(0)
                        View.Button(
                            text = "Play private match",
                            command = (fun () -> dispatch MainMenuMsg.NavigateToPrivateGame),
                            cornerRadius = 10
                        ).Row(3).Column(1)
                     ]))

