namespace BigTacToe.Pages

open Fabulous
open Fabulous.XamarinForms
open Xamarin.Forms
open Xamarin.Forms.PlatformConfiguration.TizenSpecific
open Xamarin.Forms.PlatformConfiguration.iOSSpecific

type HelpMsg =
    | GoToMainMenu

type HelpExternalMsg =
    | NoOp
    | NavigateToMainMenu

module internal Help =
    let init () = obj, Cmd.none
    
    let update message model =
        match message with
        | GoToMainMenu ->
            model, Cmd.none, HelpExternalMsg.NavigateToMainMenu
        | _ ->
            model, Cmd.none, HelpExternalMsg.NoOp

    let view model dispatch =
        ContentPage.contentPage [
            ContentPage.HasNavigationBar false
            ContentPage.Content <|
                Grid.grid [
                    Grid.Padding 20.0
                    Grid.Rows [ Star; Absolute 50.0 ]
                    Grid.Children [
                        StackLayout.stackLayout [
                            StackLayout.Children [
                                Label.label [
                                    Label.FontSize <| FontSize.Size 36.0
                                    Label.Text "How to play"
                                ]
                                Label.label [
                                    Label.FontSize <| FontSize.Size 18.0
                                    Label.Text "Win three small boards in a row. Each small board is its own self-contained Tic Tac Toe board."
                                ]
                                Label.label [
                                    Label.FontSize <| FontSize.Size 18.0
                                    Label.Text "Each turn, you play on one of the squares within a small board."
                                ]
                                Label.label [
                                    Label.FontSize <| FontSize.Size 18.0
                                    Label.Text "When you get three in a row in a small board, you’ve won that board."
                                ]
                                Label.label [
                                    Label.FontSize <| FontSize.Size 18.0
                                    Label.Text "You don’t get to pick which of the nine boards to play on. That’s determined by your opponent’s previous move. Whichever position they pick within the small board, that’s the position of the small board in the main board you must play in next."
                                ]
                                Label.label [
                                    Label.FontSize <| FontSize.Size 18.0
                                    Label.Text "If your opponent sends you to a board that's already been won, you may play anywhere."
                                ]
                            ]
                        ]
                        Button.button [
                            Button.Row 1
                            Button.Text "Back"
                            Button.FontSize <| FontSize.Size 24.0
                            Button.OnClick (fun () -> dispatch HelpMsg.GoToMainMenu)
                        ]
                    ]
                ]
        ]
