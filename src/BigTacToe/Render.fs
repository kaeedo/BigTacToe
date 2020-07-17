﻿namespace BigTacToe

open SkiaSharp
open SkiaSharp.Views.Forms



[<RequireQualifiedAccess>]
module Render =
    module private Colors =
        let largeGridDivider = SKColor.Parse("#00F")
        let gray = SKColor.Parse("#222f")
        let tileBorder = SKColor.Parse("#000")
        let meepleEx = SKColor.Parse("#00F")
        let meepleOh = SKColor.Parse("#00F")

    let private largeStroke = 10.0f
    let private smallStroke = 5.0f

    let private drawEx (canvas: SKCanvas) (rect: SKRect) =
        use paint =
            new SKPaint(Color = Colors.meepleEx, StrokeWidth = largeStroke, IsStroke = true)

        let paddingHorizontal = rect.Width * 0.1f
        let paddingVertical = rect.Height * 0.1f
        let startX = rect.Left + paddingHorizontal
        let startY = rect.Top + paddingVertical
        let endX = rect.Right - paddingHorizontal
        let endY = rect.Bottom - paddingVertical

        canvas.DrawLine(startX, startY, endX, endY, paint)
        canvas.DrawLine(endX, startY, startX, endY, paint)

    let private drawOh (canvas: SKCanvas) (rect: SKRect) =
        use paint =
            new SKPaint(Color = Colors.meepleOh, StrokeWidth = smallStroke, IsStroke = true)

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

    let drawBoard (args: SKPaintSurfaceEventArgs) (board: Board) =
        use canvas = args.Surface.Canvas

        use paint =
            new SKPaint(Color = Colors.largeGridDivider, StrokeWidth = largeStroke, IsStroke = true)

        use smallPaint =
            new SKPaint(Color = Colors.tileBorder, StrokeWidth = smallStroke, IsStroke = true)

        use transparentPaint = new SKPaint(Color = Colors.gray)

        board.SubBoards
        |> Array2D.iter (fun sb ->
            sb.Tiles
            |> Array2D.iter (fun (rect, _) -> canvas.DrawRect(rect, smallPaint))

            canvas.DrawRect(sb.Rect, paint)
            if not sb.IsPlayable
            then canvas.DrawRect(sb.Rect, transparentPaint)

            match sb.Winner with
            | None -> ()
            | Some meeple -> if meeple = Meeple.Ex then drawEx canvas sb.Rect else drawOh canvas sb.Rect)

    let drawMeeple (args: SKPaintSurfaceEventArgs) (board: Board) =
        use canvas = args.Surface.Canvas

        board.SubBoards
        |> Array2D.iter (fun sb ->
            sb.Tiles
            |> Array2D.iter (fun (rect, meeple) ->
                meeple
                |> Option.iter (fun m ->
                    match m with
                    | Meeple.Ex -> drawEx canvas rect
                    | Meeple.Oh -> drawOh canvas rect)))
