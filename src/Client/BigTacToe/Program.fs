namespace BigTacToe

open BigTacToe.Pages

open Fabulous
open Fabulous.XamarinForms
open Xamarin.Forms
open BigTacToe.Shared
open Fable.SignalR.Elmish
open System

module private App =
    type Model =
        { PlayerId: Guid
          MainMenuPageModel: obj
          HelpPageModel: obj option
          GamePageModel: ClientGameModel option }

    type Pages =
        { MainMenuPage: ViewElement
          HelpPage: ViewElement option
          GamePage: ViewElement option }

    type Msg =
        | MainMenuPageMsg of MainMenuMsg
        | GamePageMsg of GameMsg
        | HelpPageMsg of HelpMsg

        | GoToAiGame
        | GoToLocalGame
        | GoToMatchmakingGame
        | GoToPrivateGame

        | GoToMainMenu
        
        | GoToHelp

        | NavigationPopped

    let handleMainExternalMsg externalMsg =
        match externalMsg with
        | MainMenuExternalMsg.NoOp -> Cmd.none
        | MainMenuExternalMsg.NavigateToGame opponent ->
            match opponent with
            | Ai -> Cmd.ofMsg GoToAiGame
            | HotSeat -> Cmd.ofMsg GoToLocalGame
            | Random -> Cmd.ofMsg GoToMatchmakingGame
            | Private -> Cmd.ofMsg GoToPrivateGame
        | MainMenuExternalMsg.NavigateToHelp -> Cmd.ofMsg GoToHelp

    let handleGameExternalMsg externalMsg =
        match externalMsg with
        | GameExternalMsg.NoOp -> Cmd.none
        | GameExternalMsg.NavigateToMainMenu -> Cmd.ofMsg GoToMainMenu
        
    let handleHelpExternalMsg externalMsg =
        match externalMsg with
        | HelpExternalMsg.NoOp -> Cmd.none
        | HelpExternalMsg.NavigateToMainMenu -> Cmd.ofMsg GoToMainMenu

    let init () =
        let playerId =
#if __ANDROID__
            let guid = Xamarin.Essentials.Preferences.Get("PlayerId", Guid.NewGuid().ToString())
            if not <| Xamarin.Essentials.Preferences.ContainsKey("PlayerId")
            then Xamarin.Essentials.Preferences.Set("PlayerId", guid)
            Guid.Parse(guid)
#else
            Guid.NewGuid()
#endif
            
        let mainMenuPageModel, mainPageMessage = MainMenu.init ()

        let pages =
            { Model.PlayerId = playerId
              MainMenuPageModel = mainMenuPageModel
              HelpPageModel = None
              GamePageModel = None }

        pages, (Cmd.map MainMenuPageMsg mainPageMessage)


    let popNavigationPage (model: Model) =
        match model.GamePageModel, model.HelpPageModel with
        | None, None _ -> model
        | None, Some _ -> { model with HelpPageModel = None }
        | Some _, None -> { model with GamePageModel = None }
        | Some _, Some _ -> { model with GamePageModel = None; HelpPageModel = None }

    let update msg model =
        let playerId = model.PlayerId
        match msg with
        | MainMenuPageMsg msg ->
            let m, cmd, externalMsg =
                MainMenu.update msg model.MainMenuPageModel

            let cmd2 = handleMainExternalMsg externalMsg

            { model with MainMenuPageModel = m },
            Cmd.batch [ (Cmd.map MainMenuPageMsg cmd)
                        cmd2 ]
        | GamePageMsg msg ->
            let m, cmd, externalMsg =
                Messages.update msg model.GamePageModel.Value

            let cmd2 = handleGameExternalMsg externalMsg

            { model with GamePageModel = Some m },
            Cmd.batch [ (Cmd.map GamePageMsg cmd)
                        cmd2 ]
        | HelpPageMsg msg ->
            let m, cmd, externalMsg = Help.update msg model.HelpPageModel.Value
            
            let cmd2 = handleHelpExternalMsg externalMsg
            
            { model with HelpPageModel = Some m },
            Cmd.batch [(Cmd.map HelpPageMsg cmd); cmd2]

        | NavigationPopped -> popNavigationPage model, Cmd.none
        | GoToAiGame ->
            let participant =
                { Participant.PlayerId = playerId
                  Meeple = Meeple.Ex }

            let opponent =
                { Participant.PlayerId = Guid.NewGuid()
                  Meeple = Meeple.Oh }

            let newGm =
                GameModel.init (TwoPlayers(participant, opponent))

            let newGm, cmd = newGm, Cmd.none

            let m =
                { Size = 100
                  GameModel = newGm
                  OpponentStatus = LocalAiGame
                  GameIdText = String.Empty
                  Canvas = ViewRef<SkiaSharp.Views.Forms.SKCanvasView>()
                  RunningAnimation = None
                  Hub = None
                  MyStatus = participant
                  GameId = 0 }

            { model with GamePageModel = Some m }, (Cmd.map GamePageMsg cmd)
        | GoToLocalGame ->
            let participant =
                { Participant.PlayerId = playerId
                  Meeple = Meeple.Ex }

            let opponent =
                { Participant.PlayerId = Guid.NewGuid()
                  Meeple = Meeple.Oh }

            let newGm =
                GameModel.init (TwoPlayers(participant, opponent))

            let newGm, cmd = newGm, Cmd.none

            let m =
                { Size = 100
                  GameModel = newGm
                  OpponentStatus = LocalGame
                  GameIdText = String.Empty
                  Canvas = ViewRef<SkiaSharp.Views.Forms.SKCanvasView>()
                  RunningAnimation = None
                  Hub = None
                  MyStatus = participant
                  GameId = 0 }

            { model with GamePageModel = Some m }, (Cmd.map GamePageMsg cmd)
        | GoToMatchmakingGame ->
            let participant =
                { Participant.PlayerId = playerId
                  Meeple = Meeple.Ex }

            let newGm = GameModel.init (OnePlayer participant)
            let newGm, cmd = newGm, Cmd.ofMsg ConnectToServer

            let m =
                { Size = 100
                  GameModel = newGm
                  OpponentStatus = LookingForGame
                  GameIdText = String.Empty
                  Canvas = ViewRef<SkiaSharp.Views.Forms.SKCanvasView>()
                  RunningAnimation = None
                  Hub = None
                  MyStatus = participant
                  GameId = 0 }

            { model with GamePageModel = Some m }, (Cmd.map GamePageMsg cmd)

        | GoToPrivateGame ->
            // TODO: This
            let participant =
                { Participant.PlayerId = playerId
                  Meeple = Meeple.Ex }

            let newGm = GameModel.init (OnePlayer participant)
            let newGm, cmd = newGm, Cmd.ofMsg ConnectToServer

            let m =
                { Size = 100
                  GameModel = newGm
                  OpponentStatus = WaitingForPrivate None
                  GameIdText = String.Empty
                  Canvas = ViewRef<SkiaSharp.Views.Forms.SKCanvasView>()
                  RunningAnimation = None
                  Hub = None
                  MyStatus = participant
                  GameId = 0 }

            { model with GamePageModel = Some m }, (Cmd.map GamePageMsg cmd)
        | GoToMainMenu -> popNavigationPage { model with GamePageModel = None }, Cmd.none
        | GoToHelp -> { model with HelpPageModel = Some <| obj() }, Cmd.none
        
    let getPages (allPages: Pages) =
        let mainMenuPage = allPages.MainMenuPage
        let gamePage = allPages.GamePage
        let helpPage = allPages.HelpPage

        match helpPage, gamePage with
        | None, None -> [ mainMenuPage ]
        | Some hp, None -> [ mainMenuPage; hp ]
        | None, Some gp -> [ mainMenuPage; gp ]
        | Some hp, Some gp -> [ mainMenuPage; hp;  gp ]

    let view (model: Model) dispatch =
        let mainMenuPage =
            MainMenu.view model.MainMenuPageModel (MainMenuPageMsg >> dispatch)

        let gamePage =
            model.GamePageModel
            |> Option.map (fun gpm -> Game.view gpm (GamePageMsg >> dispatch))
            
        let helpPage =
            model.HelpPageModel
            |> Option.map (fun hpm -> Help.view hpm (HelpPageMsg >> dispatch))

        let allPages =
            { Pages.MainMenuPage = mainMenuPage
              HelpPage = helpPage
              GamePage = gamePage }

        NavigationPage.navigationPage [
                NavigationPage.OnPopped (fun _ -> dispatch NavigationPopped)
                NavigationPage.Pages <| getPages allPages
            ]            

    let program = Program.mkProgram init update view


type App() as app =
    inherit Application()

    let runner =
        App.program
#if DEBUG
        //|> Program.withConsoleTrace
#endif
        |> XamarinFormsProgram.run app
        

