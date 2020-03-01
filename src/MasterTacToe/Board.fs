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
        let cellWidth = width / 9.0f
        let cellHeight = height / 9.0f

        seq {
            for i in 0.0f..8.0f do
                for j in 0.0f..8.0f do
                    let left = cellWidth * i
                    let top = cellHeight * j
                    let right = left + cellWidth
                    let bottom = top + cellHeight
                    yield SKRect(left, top, right, bottom)
        }
        |> Seq.tryFind (fun rect ->
            rect.Contains(touchPoint)
        )
        |> Option.iter (fun rect ->
            use squarePaint = new SKPaint(Color = SKColor.Parse("#F00"))
            args.Surface.Canvas.DrawRect(rect, squarePaint)
        )
