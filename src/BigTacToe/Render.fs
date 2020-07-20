namespace BigTacToe

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
        let gameDraw = SKColor.Parse("#00F")

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
            | Player m -> if m = Meeple.Ex then drawEx canvas rect else drawOh canvas rect
            | Draw -> drawGameDraw canvas rect)


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

            // Draw sub board borders
            canvas.DrawRect(sb.Rect, paint)

            // Draw playability grey out
            if not sb.IsPlayable
            then canvas.DrawRect(sb.Rect, transparentPaint)


            drawWinner sb.Winner canvas sb.Rect)

        // draw main winner
        drawWinner board.Winner canvas (SKRect(0.0f, 0.0f, float32 board.Size.Width, float32 board.Size.Height))

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
