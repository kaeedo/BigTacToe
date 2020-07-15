namespace BigTacToe

open SkiaSharp
open SkiaSharp.Views.Forms

[<RequireQualifiedAccess>]
module SKBoard =
    let private largeStroke = 10.0f
    let private smallStroke = 5.0f

    let drawBoard (args: SKPaintSurfaceEventArgs) (board: Board) =
        use canvas = args.Surface.Canvas

        use paint =
            new SKPaint(Color = SKColor.Parse("#00F"), StrokeWidth = largeStroke, IsStroke = true)

        use smallPaint =
            new SKPaint(Color = SKColor.Parse("#000"), StrokeWidth = smallStroke, IsStroke = true)

        board.SubBoards
        |> Array2D.iter (fun sb ->
            sb.Tiles
            |> Array2D.iter (fun (rect, _) -> canvas.DrawRect(rect, smallPaint))

            canvas.DrawRect(sb.Rect, paint))

    let drawMeeple (args: SKPaintSurfaceEventArgs) (board: Board) =
        use canvas = args.Surface.Canvas

        use squarePaint =
            new SKPaint(Color = SKColor.Parse("#F00"))

        let drawEx (rect: SKRect) =
            use paint =
                new SKPaint(Color = SKColor.Parse("#00F"), StrokeWidth = largeStroke, IsStroke = true)

            let paddingHorizontal = rect.Width * 0.1f
            let paddingVertical = rect.Height * 0.1f
            let startX = rect.Left + paddingHorizontal
            let startY = rect.Top + paddingVertical
            let endX = rect.Right - paddingHorizontal
            let endY = rect.Bottom - paddingVertical

            canvas.DrawLine(startX, startY, endX, endY, paint)
            canvas.DrawLine(endX, startY, startX, endY, paint)

        let drawOh (rect: SKRect) =
            use paint =
                new SKPaint(Color = SKColor.Parse("#00F"), StrokeWidth = smallStroke, IsStroke = true)

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

        board.SubBoards
        |> Array2D.iter (fun sb ->
            sb.Tiles
            |> Array2D.iter (fun (rect, meeple) ->
                meeple
                |> Option.iter (fun m ->
                    match m with
                    | Meeple.Ex -> drawEx rect
                    | Meeple.Oh -> drawOh rect)))
