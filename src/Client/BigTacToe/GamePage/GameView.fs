﻿namespace BigTacToe.Pages

open BigTacToe
open BigTacToe.Pages

open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.SkiaSharp
open Xamarin.Forms
open BigTacToe.Shared

[<RequireQualifiedAccess>]
module internal Game =
    let view (model: ClientGameModel) dispatch =
        let gm = model.GameModel

        let gameStatus =
            match gm.Board.Winner with
            | None -> sprintf "It is %s's turn to play" (gm.CurrentPlayer.ToString())
            | Some w ->
                match w with
                | Draw -> "It's a tie game. Nobody wins"
                | Participant p -> sprintf "%s wins!" (p.ToString())
            

        let gameBoard =
            View.StackLayout
                (children = [
                    dependsOn (model.Size, gm.Board) (fun _ (size, board) ->
                        View.SKCanvasView
                            (invalidate = true,
                            enableTouchEvents = true,
                            verticalOptions = LayoutOptions.FillAndExpand,
                            horizontalOptions = LayoutOptions.FillAndExpand,
                            paintSurface =
                                (fun args ->
                                    dispatch <| ResizeCanvas args.Info.Size
                         
                                    args.Surface.Canvas.Clear()
                         
                                    Render.drawBoard args model
                                    Render.drawMeeple args model),
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
                            View.Label(text = gameStatus, fontSize = FontSize.Size 24.0, horizontalTextAlignment = TextAlignment.Center)
                            gameBoard.Row(1)
                            View.Label(text = "AI thinking", horizontalTextAlignment = TextAlignment.Center).Row(2)
                            ]))

        page