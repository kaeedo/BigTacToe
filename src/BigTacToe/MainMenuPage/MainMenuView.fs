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
        | Increment -> { model with Count = model.Count + 1}, Cmd.none, MainMenuExternalMsg.NoOp
        | Decrement -> { model with Count = model.Count - 1}, Cmd.none, MainMenuExternalMsg.NoOp
        | MainMenuMsg.NavigateToGame -> model, Cmd.none, MainMenuExternalMsg.NavigateToGame

    let view model dispatch =
        View.ContentPage
            (content =
                View.Grid
                    (rowdefs = [Absolute 50.0; Star; Absolute 50.0],
                     coldefs = [Star; Star],
                     padding = Thickness 20.0,
                     ref = ViewRef<Grid>(), //model.GridLayout,
                     children = [
                        View.Button(
                            text = "+",
                            command = (fun () -> dispatch Increment) 
                        ).Row(0).BackgroundColor(Color.Green)
                        View.Button(
                            text = "-",
                            command = (fun () -> dispatch Decrement) 
                        ).Row(0).Column(1).BackgroundColor(Color.Red)
                        View.Label(text = model.Count.ToString()).Row(1)
                        View.Button(
                            text = "Game",
                            command = (fun () -> dispatch MainMenuMsg.NavigateToGame)
                        ).Row(2).ColumnSpan(2)
                     ]))

