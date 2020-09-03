namespace BigTacToe.Pages

open BigTacToe
open BigTacToe.Pages

open Fabulous
open Fabulous.XamarinForms
open Xamarin.Forms

[<RequireQualifiedAccess>]
module internal Game =
    let view (model: GameModel) dispatch =
        let gameStatus =
            match model.Board.Winner with
            | None -> sprintf "It is %s's turn to play" (model.CurrentPlayer.ToString())
            | Some w ->
                match w with
                | Draw -> "It's a tie game. Nobody wins"
                | Player p -> sprintf "%s wins!" (p.ToString())
            

        let gameBoard =
            View.StackLayout
                (children = [
                    dependsOn (model.Size, model.Board) (fun _ (size, board) ->
                        View.SKCanvasView
                            (invalidate = true,
                            enableTouchEvents = true,
                            verticalOptions = LayoutOptions.FillAndExpand,
                            horizontalOptions = LayoutOptions.FillAndExpand,
                            paintSurface =
                                (fun args ->
                                    dispatch <| ResizeCanvas args.Info.Size
                         
                                    args.Surface.Canvas.Clear()
                         
                                    Render.drawBoard args board
                                    Render.drawMeeple args board),
                            touch =
                                (fun args ->
                                    if args.InContact
                                    then dispatch (SKSurfaceTouched args.Location))))
                ])
        
        let page =
            View.ContentPage
                (content =
                    View.Grid
                        (rowdefs = [Absolute 50.0; Star; Absolute 50.0],
                            coldefs = [Star],
                            padding = Thickness 20.0,
                            ref = ViewRef<Grid>(), //model.GridLayout,
                            children = [
                            View.Label(text = gameStatus, fontSize = FontSize 24.0, horizontalTextAlignment = TextAlignment.Center)
                            gameBoard.Row(1)
                            View.Label(text = "AI thinking", horizontalTextAlignment = TextAlignment.Center).Row(2)
                            ]))

        page