namespace BigTacToe.Pages

open Xamarin.Forms
open Fabulous

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
                Button.button [
                    Button.Text "wefewf"
                    Button.OnClick (fun () -> dispatch HelpMsg.GoToMainMenu)
                ]
        ]
