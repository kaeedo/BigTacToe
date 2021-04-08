namespace BigTacToe.Pages

open Fabulous.XamarinForms
open Xamarin.Forms
open Fabulous
open BigTacToe.Components

[<RequireQualifiedAccess>]
module internal MainMenu =
    let fontSize = FontSize.Size 18.0
    
    let init () = MainMenuModel(), Cmd.none

    let update message model =
        match message with
        | MainMenuMsg.NavigateToAiGame -> model, Cmd.none, MainMenuExternalMsg.NavigateToGame Ai
        | MainMenuMsg.NavigateToHotSeatGame -> model, Cmd.none, MainMenuExternalMsg.NavigateToGame HotSeat
        | MainMenuMsg.NavigateToMatchmakingGame -> model, Cmd.none, MainMenuExternalMsg.NavigateToGame Random
        | MainMenuMsg.NavigateToPrivateGame -> model, Cmd.none, MainMenuExternalMsg.NavigateToGame Private
        | MainMenuMsg.NavigateToHelp -> model, Cmd.none, MainMenuExternalMsg.NavigateToHelp
        
    

    let view model dispatch =
        ContentPage.contentPage [
            ContentPage.HasNavigationBar false
            ContentPage.Content <|
                Grid.grid [
                    Grid.Rows [ Absolute 50.0; Star; Absolute 100.0 ]
                    Grid.Columns [ Absolute 50.0; Star; Star; Absolute 50.0 ]
                    Grid.Padding 20.0
                    Grid.Children [
                        Label.label [
                            Label.Text "Big Tac Toe"
                            Label.TextColor Color.Black
                            Label.FontSize <| FontSize.Size 40.0
                            Label.HorizontalTextAlignment TextAlignment.Center
                            Label.ColumnSpan 4
                        ]
                        Button.button [
                            Button.Text "?"
                            Button.FontSize <| FontSize.Size 24.0
                            Button.BackgroundColor Color.Transparent
                            Button.BorderColor Color.Black
                            Button.BorderWidth 1.0
                            Button.CornerRadius 50
                            Button.Width 25.0
                            Button.Height 25.0
                            Button.OnClick (fun () -> dispatch MainMenuMsg.NavigateToHelp)
                        ]
                        Image.image [
                            Image.Source <| Image.fromPath "BigTacToe.png"
                            Image.Aspect Aspect.AspectFit
                            Image.Row 1
                            Image.ColumnSpan 4
                        ]
                        Grid.grid [
                            Grid.Row 2
                            Grid.ColumnSpan 4
                            Grid.Rows [ Star; Absolute 2.0; Star; ]
                            Grid.Columns [ Star; Absolute 2.0; Star; ]
                            Grid.Padding 0.0
                            Grid.RowSpacing 0.0
                            Grid.ColumnSpacing 0.0
                            Grid.Children [
                                IconButton.iconButton [Frame.Row 0; Frame.Column 0] "cpu" "Play vs. CPU" (fun () -> dispatch MainMenuMsg.NavigateToAiGame)
                                IconButton.iconButton [Frame.Row 0; Frame.Column 2] "passPhone" "2P. offline" (fun () -> dispatch MainMenuMsg.NavigateToHotSeatGame)
                                StackLayout.stackLayout [
                                    StackLayout.Column 1
                                    StackLayout.RowSpan 3
                                    StackLayout.Padding 0.0
                                    StackLayout.Orientation StackOrientation.Horizontal
                                    StackLayout.Children [
                                        BoxView.boxView [
                                           BoxView.Width 1.0
                                           BoxView.Color <| Color.FromHex("#413D21")
                                           BoxView.VerticalLayout LayoutOptions.Fill
                                        ]
                                    ]
                                ]
                                StackLayout.stackLayout [
                                    StackLayout.Row 1
                                    StackLayout.ColumnSpan 3
                                    StackLayout.Padding 0.0
                                    StackLayout.Children [
                                        BoxView.boxView [
                                           BoxView.Height 1.0
                                           BoxView.Color <| Color.FromHex("#413D21")
                                           BoxView.HorizontalLayout LayoutOptions.Fill
                                        ]
                                    ]
                                ]
                                IconButton.iconButton [Frame.Row 2; Frame.Column 0] "matchmaking" "Find opponent" (fun () -> dispatch MainMenuMsg.NavigateToMatchmakingGame)
                                IconButton.iconButton [Frame.Row 2; Frame.Column 2] "privateGame" "Private match" (fun () -> dispatch MainMenuMsg.NavigateToPrivateGame)
                            ]
                        ]
                    ]
                ]
            ]

