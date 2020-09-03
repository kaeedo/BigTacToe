namespace BigTacToe.Pages

open Fabulous.XamarinForms
open Xamarin.Forms

open Fabulous

type MainMenuModel =
    { Count: int }

type MainMenuMsg =
| Increment
| Decrement
| NavigateToGame

type MainMenuExternalMsg =
| NoOp
| NavigateToGame

[<RequireQualifiedAccess>]
module internal MainMenu =
    let init () = { MainMenuModel.Count = 0 }, Cmd.none

    let update message model =
        match message with
        | Increment -> { model with Count = model.Count + 1 }, Cmd.none, MainMenuExternalMsg.NoOp
        | Decrement -> { model with Count = model.Count - 1 }, Cmd.none, MainMenuExternalMsg.NoOp
        | MainMenuMsg.NavigateToGame -> model, Cmd.none, MainMenuExternalMsg.NavigateToGame

    let view model dispatch =
        View.ContentPage
            (content =
                View.Grid
                    (rowdefs = [Absolute 50.0; Star; Absolute 50.0],
                     coldefs = [Star],
                     padding = Thickness 20.0,
                     //ref = ViewRef<Stack>(), //model.GridLayout,
                     children = [
                        View.Label(
                            text = "Big Tac Toe", 
                            fontSize = FontSize 36.0, 
                            horizontalTextAlignment = TextAlignment.Center
                        )
                        View.Image(
                            source = Path "placeholder.png",
                            aspect = Aspect.AspectFit
                        ).Row(1)
                        View.Button(
                            text = "Play vs. CPU",
                            command = (fun () -> dispatch MainMenuMsg.NavigateToGame),
                            cornerRadius = 10
                        ).Row(2)
                     ]))

