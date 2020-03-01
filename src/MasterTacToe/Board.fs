namespace MasterTacToe

open SkiaSharp
open SkiaSharp.Views.Forms

[<RequireQualifiedAccess>]
module Board =
    let private largeStroke = 10.0f
    let private smallStroke = 5.0f

    let drawBoard (args: SKPaintSurfaceEventArgs) =
        let info = args.Info
        let surface = args.Surface
        let canvas = surface.Canvas

        use paint = new SKPaint(Color = SKColor.Parse("#00F"), StrokeWidth = largeStroke, IsStroke = true)
        use smallPaint = new SKPaint(Color = SKColor.Parse("#000"), StrokeWidth = smallStroke, IsStroke = true)

        let width = float32 info.Width
        let height = float32 info.Height
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

    let highlightSquare (args: SKPaintSurfaceEventArgs) (touchPoint: SKPoint) =
        let width = float32 args.Info.Width
        let height = float32 args.Info.Height
        let cellWidth = (width - (6.0f * smallStroke) - (2.0f * largeStroke)) / 9.0f
        let cellWidth = (width ) / 9.0f

        let cells =
            seq {
                yield (0.0f, cellWidth)
                yield (cellWidth * 1.0f , cellWidth * 2.0f)
                yield (cellWidth * 2.0f , cellWidth * 3.0f)

                yield (cellWidth * 3.0f , cellWidth * 4.0f)
                yield (cellWidth * 4.0f , cellWidth * 5.0f)
                yield (cellWidth * 5.0f , cellWidth * 6.0f)

                yield (cellWidth * 6.0f , cellWidth * 7.0f)
                yield (cellWidth * 7.0f , cellWidth * 8.0f)
                yield (cellWidth * 8.0f , cellWidth * 9.0f)
            } |> List.ofSeq

        let index =
            cells 
            |> List.tryFindIndex (fun (startPoint, endPoint) ->
                let startPoint = SKPoint(startPoint, 0.0f)
                let endPoint = SKPoint(endPoint, 0.0f)

                touchPoint.X >= startPoint.X && touchPoint.X < endPoint.X
            )

        cells 
        |> List.tryFind (fun (startPoint, endPoint) ->
            let startPoint = SKPoint(startPoint, 0.0f)
            let endPoint = SKPoint(endPoint, 0.0f)

            touchPoint.X >= startPoint.X && touchPoint.X < endPoint.X
        )
        |> Option.iter (fun tc ->
            use squarePaint = new SKPaint(Color = SKColor.Parse("#F00"))
            args.Surface.Canvas.DrawRect(fst tc, 0.0f, cellWidth, height, squarePaint)
        )


        // 0 -> 175
        // 180 -> 355
        // 360 -> 535

        // 545 -> 720
        // 725 -> 900
        // 910 -> 1085

        // 1095 -> 1270
        // 1275 -> 1450
        // 1455 -> 1630

        // x: 830