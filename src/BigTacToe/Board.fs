﻿namespace BigTacToe

open SkiaSharp
open SkiaSharp.Views.Forms

[<RequireQualifiedAccess>]
module SKBoard =
    let private largeStroke = 10.0f
    let private smallStroke = 5.0f

    let drawBoard (args: SKPaintSurfaceEventArgs) (board: Board) =
        let canvas = args.Surface.Canvas

        use paint = new SKPaint(Color = SKColor.Parse("#00F"), StrokeWidth = largeStroke, IsStroke = true)
        use smallPaint = new SKPaint(Color = SKColor.Parse("#000"), StrokeWidth = smallStroke, IsStroke = true)

        let width = float32 board.Size.Width
        let height = float32 board.Size.Height
        let ninthWidth = width / 9.0f
        let ninthHeight = height / 9.0f

        let verticalLines =
            [ for i in 1.0f .. 8.0f do
                yield SKPoint(ninthWidth * i, 0.0f), SKPoint(ninthWidth * i, height) ]
        let horizontalLines =
            [ for i in 1.0f .. 8.0f do
                yield SKPoint(0.0f, ninthHeight * i), SKPoint(width, ninthHeight * i) ]

        List.iteri2 (fun i (startPoint1, endPoint1) (startPoint2, endPoint2) ->
            let paint = if (i + 1) % 3 = 0 then paint else smallPaint
            canvas.DrawLine(startPoint1, endPoint1, paint)
            canvas.DrawLine(startPoint2, endPoint2, paint)
        ) verticalLines horizontalLines

    let highlightSquare (args: SKPaintSurfaceEventArgs) (touchedTile: SKRect * (Meeple option)) =
        

        //Messages.calculateBoundingBoxes <| SKSizeI(args.Info.Width, args.Info.Height)
        //|> Seq.tryFind (fun rect ->
        //    rect.Contains(touchPoint)
        //)
        let rect = fst touchedTile
        use squarePaint = new SKPaint(Color = SKColor.Parse("#F00"))
        args.Surface.Canvas.DrawRect(rect, squarePaint)
