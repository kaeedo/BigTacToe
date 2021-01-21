namespace BigTacToe

open BigTacToe.Pages

open Fabulous
open Fabulous.XamarinForms
open Xamarin.Forms
open BigTacToe.Shared
open System

module private App =
    type Model =
        { MainMenuPageModel : MainMenuModel
          GamePageModel: ClientGameModel option }

    type Pages =
        { MainMenuPage: ViewElement
          GamePage: ViewElement option }

    type Msg =
        | MainMenuPageMsg of MainMenuMsg
        | GamePageMsg of GameMsg

        | GoToAiGame
        | GoToHotSeatGame
        | GoToMatchmakingGame
        | GoToPrivateGame

        | GoToMainMenu

        | NavigationPopped

    let handleMainExternalMsg externalMsg =
        match externalMsg with
        | MainMenuExternalMsg.NoOp ->
            Cmd.none
        | MainMenuExternalMsg.NavigateToGame opponent ->
            match opponent with
            | Ai -> Cmd.ofMsg GoToAiGame
            | HotSeat -> Cmd.ofMsg GoToHotSeatGame
            | Random -> Cmd.ofMsg GoToMatchmakingGame
            | Private -> Cmd.ofMsg GoToPrivateGame

    let handleGameExternalMsg externalMsg =
        match externalMsg with
        | GameExternalMsg.NoOp ->
            Cmd.none
        | GameExternalMsg.NavigateToMainMenu ->
            Cmd.ofMsg GoToMainMenu

    let init () = 
        let mainMenuPageModel, mainPageMessage = MainMenu.init ()
        //let gamePageModel, gamePageMsg = GamePage.Types.initModel (), Cmd.none

        let pages =
            { Model.MainMenuPageModel = mainMenuPageModel
              GamePageModel = None }

        pages, (Cmd.map MainMenuPageMsg mainPageMessage)


    let navigationMapper (model: Model) =
        let gameModel = model.GamePageModel
        match gameModel with
        | None ->
            model
        | Some _ ->
            { model with GamePageModel = None }

    let update msg model = 
        match msg with 
        | MainMenuPageMsg msg ->
            let m, cmd, externalMsg = MainMenu.update msg model.MainMenuPageModel
            let cmd2 = handleMainExternalMsg externalMsg
            { model with MainMenuPageModel = m }, Cmd.batch [(Cmd.map MainMenuPageMsg cmd); cmd2 ]
        | GamePageMsg msg ->
            let m, cmd = Messages.update msg model.GamePageModel.Value
            { model with GamePageModel = Some m }, (Cmd.map GamePageMsg cmd)

        | NavigationPopped ->
            navigationMapper model, Cmd.none
        | GoToAiGame ->
            let participant = { Participant.PlayerId = Guid.NewGuid(); Meeple = Meeple.Ex } // Get this from "actual" player guid
            let opponent = { Participant.PlayerId = Guid.NewGuid(); Meeple = Meeple.Oh } // Only for single player
            let newGm = GameModel.init participant
            let newGm = { newGm with Players = TwoPlayers (participant, opponent) }
            let newGm, cmd = newGm, Cmd.none
            let m = { Size = 100, 100; GameModel = newGm; OpponentStatus = LocalAiGame; Hub = None; MyStatus = participant }
            { model with GamePageModel = Some m }, (Cmd.map GamePageMsg cmd)
        | GoToHotSeatGame ->
            // TODO: this
            let participant = { Participant.PlayerId = Guid.NewGuid(); Meeple = Meeple.Ex } // Get this from "actual" player guid
            let opponent = { Participant.PlayerId = Guid.NewGuid(); Meeple = Meeple.Oh } // Only for single player
            let newGm = GameModel.init participant
            let newGm = { newGm with Players = TwoPlayers (participant, opponent) }
            let newGm, cmd = newGm, Cmd.none
            let m = { Size = 100, 100; GameModel = newGm; OpponentStatus = LocalAiGame; Hub = None; MyStatus = participant }
            { model with GamePageModel = Some m }, (Cmd.map GamePageMsg cmd)
        | GoToMatchmakingGame ->
            let participant = { Participant.PlayerId = Guid.NewGuid(); Meeple = Meeple.Ex } // Get this from "actual" player guid

            let newGm = GameModel.init participant
            let newGm = { newGm with Players = OnePlayer participant }
            let newGm, cmd = newGm, Cmd.ofMsg ConnectToServer
            let m = { Size = 100, 100; GameModel = newGm; OpponentStatus = LookingForGame; Hub = None; MyStatus = participant }
            { model with GamePageModel = Some m }, (Cmd.map GamePageMsg cmd)

        | GoToPrivateGame ->
            // TODO: This
            let participant = { Participant.PlayerId = Guid.NewGuid(); Meeple = Meeple.Ex } // Get this from "actual" player guid

            let newGm = GameModel.init participant
            let newGm = { newGm with Players = OnePlayer participant }
            let newGm, cmd = newGm, Cmd.none
            let m = { Size = 100, 100; GameModel = newGm; OpponentStatus = WaitingForPrivate -1; Hub = None; MyStatus = participant }
            { model with GamePageModel = Some m }, (Cmd.map GamePageMsg cmd)

    let getPages (allPages: Pages) =
        let mainMenuPage = allPages.MainMenuPage
        let gamePage = allPages.GamePage
        
        match gamePage with
        | None -> [ mainMenuPage ]
        | Some gp -> [ mainMenuPage; gp ]


    let view (model: Model) dispatch =
        let mainMenuPage = MainMenu.view model.MainMenuPageModel (MainMenuPageMsg >> dispatch)
        let gamePage = 
            model.GamePageModel
            |> Option.map (fun gpm ->
                Game.view gpm (GamePageMsg >> dispatch)
            )
        
        let allPages = 
            { Pages.MainMenuPage = mainMenuPage
              GamePage = gamePage }

        View.NavigationPage(
            hasNavigationBar = false,
            popped = (fun _ -> dispatch NavigationPopped),
            pages = getPages allPages
        )

    // Note, this declaration is needed if you enable LiveUpdate
    let program =
        Program.mkProgram init update view

type App() as app =
    inherit Application()

    let runner =
        App.program
#if DEBUG
        //|> Program.withConsoleTrace
#endif
        |> XamarinFormsProgram.run app

#if DEBUG
    // Uncomment this line to enable live update in debug mode.
    // See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/tools.html#live-update for further  instructions.
    //
    //do runner.EnableLiveUpdate()
#endif
  

// Uncomment this code to save the application state to app.Properties using Newtonsoft.Json
// See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/models.html#saving-application-state for further  instructions.
(*
    let modelId = "model"

    override __.OnSleep() =

        let json =
            Newtonsoft.Json.JsonConvert.SerializeObject(runner.CurrentModel)

        Console.WriteLine("OnSleep: saving model into app.Properties, json = {0}", json)

        app.Properties.[modelId] <- json

    override __.OnResume() =
        Console.WriteLine "OnResume: checking for model in app.Properties"
        try
            match app.Properties.TryGetValue modelId with
            | true, (:? string as json) ->

                Console.WriteLine("OnResume: restoring model from app.Properties, json = {0}", json)

                let model =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<Model>(json)

                Console.WriteLine("OnResume: restoring model from app.Properties, model = {0}", (sprintf "%0A" model))
                runner.SetCurrentModel(model, Cmd.none)

            | _ -> ()
        with ex -> App.program.onError ("Error while restoring model found in app.Properties", ex)

    override this.OnStart() =
        Console.WriteLine "OnStart: using same logic as OnResume()"
        this.OnResume()
        *)