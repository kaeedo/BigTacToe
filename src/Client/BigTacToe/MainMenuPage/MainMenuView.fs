namespace BigTacToe.Pages

open System
open BigTacToe.Pages
open Fabulous.XamarinForms
open Xamarin.Forms
open Fabulous
open BigTacToe.Components
open System.Net.Http

[<RequireQualifiedAccess>]
module internal MainMenu =
    let private httpClient = new HttpClient()
    
    let private fontSize = FontSize.Size 18.0
    
    let init () = MainMenuModel(), Cmd.none

    let update message model =
        match message with
        | MainMenuMsg.CheckServer opponent ->
            let serverCheck =
                let url = BigTacToe.Resources.url + "/status"
                let canReach =
                    async {
                        try
                            httpClient.Timeout <- TimeSpan.FromSeconds(5.0)
                            let! result = httpClient.GetAsync(url) |> Async.AwaitTask
                            if result.IsSuccessStatusCode
                            then
                                let! content = result.Content.ReadAsStringAsync() |> Async.AwaitTask
                                return CheckServerResponse (content, opponent)
                            else return CheckServerFailed
                        with _ -> return CheckServerFailed
                    }
                canReach
                
            model, Cmd.ofAsyncMsg serverCheck, MainMenuExternalMsg.NoOp
        | MainMenuMsg.CheckServerResponse ("Ok", opponent) ->
            model, Cmd.ofMsg (MainMenuMsg.NavigateToOnlineGame opponent), MainMenuExternalMsg.NoOp
        | MainMenuMsg.CheckServerResponse _
        | MainMenuMsg.CheckServerFailed ->
            let display =
                async {
                    do! Application.Current.MainPage.DisplayAlert("Connection problem", "Couldn't connect to server. Please try again later", "Ok") |> Async.AwaitTask
                    return None
                }
                
            model, Cmd.ofAsyncMsgOption display, MainMenuExternalMsg.NoOp
        | MainMenuMsg.NavigateToAiGame -> model, Cmd.none, MainMenuExternalMsg.NavigateToGame Ai
        | MainMenuMsg.NavigateToHotSeatGame -> model, Cmd.none, MainMenuExternalMsg.NavigateToGame HotSeat
        | MainMenuMsg.NavigateToOnlineGame Opponent.Random -> model, Cmd.none, MainMenuExternalMsg.NavigateToGame Random
        | MainMenuMsg.NavigateToOnlineGame Opponent.Private -> model, Cmd.none, MainMenuExternalMsg.NavigateToGame Private
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
                            Grid.Rows [ Star; Star; ]
                            Grid.Columns [ Star; Star; ]
                            Grid.Padding 2.0
                            Grid.RowSpacing 4.0
                            Grid.ColumnSpacing 4.0
                            Grid.Children [
                                IconButton.iconButton [Frame.Row 0; Frame.Column 0] "cpu" "Play vs. CPU" (fun () -> dispatch MainMenuMsg.NavigateToAiGame)
                                IconButton.iconButton [Frame.Row 0; Frame.Column 1] "passPhone" "2P. offline" (fun () -> dispatch MainMenuMsg.NavigateToHotSeatGame)
                                IconButton.iconButton [Frame.Row 1; Frame.Column 0] "matchmaking" "Find opponent" (fun () -> dispatch (MainMenuMsg.CheckServer Random))
                                IconButton.iconButton [Frame.Row 1; Frame.Column 1] "privateGame" "Private match" (fun () -> dispatch (MainMenuMsg.CheckServer Private))
                            ]
                        ]
                    ]
                ]
            ]

