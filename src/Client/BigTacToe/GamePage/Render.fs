﻿namespace BigTacToe.Pages

open BigTacToe.Shared
open SkiaSharp
open SkiaSharp.Views.Forms
open Xamarin.Essentials
open Fabulous.XamarinForms

[<RequireQualifiedAccess>]
module internal Render =
    module private Colors =
        let largeGridDivider = SKColor.Parse("#00F")
        let oddGridBackground = SKColor.Parse("#fff")
        let evenGridBackground = SKColor.Parse("#ececec")
        let highlight = SKColor.Parse("#ff6")
        let tileBorder = SKColor.Parse("#000")
        let meepleEx = SKColor.Parse("#F00")
        let meepleOh = SKColor.Parse("#0A0")
        let gameDraw = SKColor.Parse("#00F")
        let gameWinner = SKColor.Parse("#f00")

    let private largeStroke = 5.0f
    let private smallStroke = 2.0f

    let private drawEx (color: SKColor) (canvas: SKCanvas) (rect: SKRect) =
        use paint =
            new SKPaint(Color = color, StrokeWidth = largeStroke, IsStroke = true)

        let paddingHorizontal = rect.Width * 0.1f
        let paddingVertical = rect.Height * 0.1f
        let startX = rect.Left + paddingHorizontal
        let startY = rect.Top + paddingVertical
        let endX = rect.Right - paddingHorizontal
        let endY = rect.Bottom - paddingVertical

        canvas.DrawLine(startX, startY, endX, endY, paint)
        canvas.DrawLine(endX, startY, startX, endY, paint)

    let private drawOh (color: SKColor) (canvas: SKCanvas) (rect: SKRect) =
        use paint =
            new SKPaint(Color = color, StrokeWidth = 3.0f, IsStroke = true)

        let paddingHorizontal = rect.Width * 0.1f
        let paddingVertical = rect.Height * 0.1f
        let cx = rect.MidX
        let cy = rect.MidY

        let radius =
            if rect.Width < rect.Height then
                (rect.Width / 2.0f) - paddingHorizontal
            else
                (rect.Height / 2.0f) - paddingVertical

        canvas.DrawCircle(cx, cy, radius, paint)

    let private drawGameDraw (canvas: SKCanvas) (rect: SKRect) =
        use paint =
            new SKPaint(Color = Colors.gameDraw, StrokeWidth = smallStroke, IsStroke = true)

        let paddingHorizontal = rect.Width * 0.1f
        let paddingVertical = rect.Height * 0.1f

        canvas.DrawRect
            (rect.Left + paddingHorizontal,
             rect.Top + paddingVertical,
             rect.Width - paddingHorizontal * 2.0f,
             rect.Height - paddingVertical * 2.0f,
             paint)

    let private drawWinner winner canvas rect =
        winner
        |> Option.iter (fun w ->
            match w with
            | Participant (Player (_, m)) -> 
                if m = Meeple.Ex 
                then drawEx Colors.gameWinner canvas rect 
                else drawOh Colors.gameWinner canvas rect
            | Draw -> drawGameDraw canvas rect
            | _ -> ()
        )

    let private calculateSubBoardRect i j (width, height) =
        let (width, height) = width / 3, height / 3
        let left = width * i
        let top = height * j
        let right = left + width
        let bottom = top + height

        (left, top, right, bottom)

    let private calculateTileRect (tileI, tileJ) (subBoardRect: SKRect) =
        let iInSubBoard = tileI % 3
        let jInSubBoard = tileJ % 3

        let parentRect = subBoardRect
        let subSize =
            SKSizeI(int <| parentRect.Width / 3.0f, int <| parentRect.Height / 3.0f)

        let left =
            parentRect.Left + float32 (subSize.Width * iInSubBoard)

        let right = left + float32 subSize.Width

        let top =
            parentRect.Top + float32 (subSize.Height * jInSubBoard)

        let bottom = top + float32 subSize.Height

        SKRect(left, top, right, bottom)

    let drawBoard (args: SKPaintSurfaceEventArgs) (clientGameModel: ClientGameModel) =
        let board = clientGameModel.GameModel.Board
        use canvas = args.Surface.Canvas

        use paint =
            new SKPaint(Color = Colors.largeGridDivider, StrokeWidth = largeStroke, IsStroke = true)

        use oddGridPaint =
            new SKPaint(Color = Colors.oddGridBackground)

        use evenGridPaint =
            new SKPaint(Color = Colors.evenGridBackground)

        use smallPaint =
            new SKPaint(Color = Colors.tileBorder, StrokeWidth = smallStroke, IsStroke = true)

        use transparentPaint = 
            let p = new SKPaint(Color = Colors.highlight.WithAlpha(80uy))
            //p.MaskFilter <- SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 2.0f);
            p
            
        let isFreeMove =
            board.SubBoards
            |> Seq.cast<SubBoard>
            |> Seq.filter (fun sb -> sb.IsPlayable)
            |> Seq.length > 1

        board.SubBoards
        |> Array2D.iteri (fun i j sb ->
            // Extract this everywhere
            let (left, top, right, bottom) = calculateSubBoardRect i j clientGameModel.Size
            let subBoardRect = SKRect(float32 left, float32 top, float32 right, float32 bottom)

            let isOddGrid = ((i + j) * (i + j)) % 2 = 0
            
            // Draw sub board borders
            //canvas.Scale(2.0f)
            canvas.DrawRect(subBoardRect, if isOddGrid then oddGridPaint else evenGridPaint)

            sb.Tiles
            |> Array2D.iter (fun (globalIndex, _) -> 
                let tileRect = calculateTileRect globalIndex subBoardRect
                canvas.DrawRect(tileRect, smallPaint)
            )

            // Draw playability grey out
            if not isFreeMove && sb.IsPlayable
            then
                canvas.DrawRect(subBoardRect, transparentPaint)

            drawWinner sb.Winner canvas subBoardRect)

        // draw main winner
        let (width, height) = clientGameModel.Size
        let constrainedSize = if width > height then height else width
        drawWinner board.Winner canvas (SKRect(0.0f, 0.0f, float32 constrainedSize, float32 constrainedSize))

    let drawMeeple (args: SKPaintSurfaceEventArgs) (clientGameModel: ClientGameModel) =
        let board = clientGameModel.GameModel.Board
        use canvas = args.Surface.Canvas

        board.SubBoards
        |> Array2D.iteri (fun i j sb ->
            let (left, top, right, bottom) = calculateSubBoardRect i j clientGameModel.Size
            let subBoardRect = SKRect(float32 left, float32 top, float32 right, float32 bottom)

            sb.Tiles
            |> Array2D.iter (fun (globalIndex, meeple) ->
                //let (left, top, right, bottom) = rect
                let tileRect = calculateTileRect globalIndex subBoardRect

                meeple
                |> Option.iter (fun m ->
                    match m with
                    | Player (_, Meeple.Ex) -> drawEx Colors.meepleEx canvas tileRect
                    | Player (_, Meeple.Oh) -> drawOh Colors.meepleOh canvas tileRect
                    | Missing -> ()
                )
            )
        )
