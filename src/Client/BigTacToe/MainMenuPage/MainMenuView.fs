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
        ContentPage.contentPage [
            ContentPage.HasNavigationBar false
            ContentPage.Content <|
                Grid.grid [
                    Grid.Rows [Absolute 50.0; Star; Absolute 50.0; Absolute 50.0]
                    Grid.Columns [Star; Star]
                    Grid.Padding 20.0
                    Grid.Children [
                        Label.label [
                            Label.Text "Big Tac Toe"
                            Label.FontSize <| FontSize.Size 36.0
                            Label.HorizontalTextAlignment TextAlignment.Center
                            Label.ColumnSpan 2
                        ]
                        Image.image [
                            Image.Source <| Image.fromPath "placeholder.png"
                            Image.Aspect Aspect.AspectFit
                            Image.Row 1
                            Image.ColumnSpan 2
                        ]
                        Button.button [
                            Button.Text "Play vs. CPU"
                            Button.OnClick (fun () -> dispatch MainMenuMsg.NavigateToAiGame)
                            Button.Row 2
                            Button.Column 0
                        ]
                        Button.button [
                            Button.Text "Pass the phone"
                            Button.OnClick (fun () -> dispatch MainMenuMsg.NavigateToHotSeatGame)
                            Button.Row 2
                            Button.Column 1
                        ]
                        Button.button [
                            Button.Text "Find opponent"
                            Button.OnClick (fun () -> dispatch MainMenuMsg.NavigateToMatchmakingGame)
                            Button.Row 3
                            Button.Column 0
                        ]
                        Button.button [
                            Button.Text "Play private match"
                            Button.OnClick (fun () -> dispatch MainMenuMsg.NavigateToPrivateGame)
                            Button.Row 3
                            Button.Column 1
                        ]
                    ]
                ]
        ]

