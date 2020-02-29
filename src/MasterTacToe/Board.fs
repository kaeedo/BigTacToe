namespace MasterTacToe

open SkiaSharp
open SkiaSharp.Views.Forms

[<RequireQualifiedAccess>]
module Board =
    let drawBoard (args: SKPaintSurfaceEventArgs) =
        let info = args.Info
        let surface = args.Surface
        let canvas = surface.Canvas

        use paint = new SKPaint(Color = SKColor.Parse("#00F"), StrokeWidth = 10.0f, IsStroke = true)
        use smallPaint = new SKPaint(Color = SKColor.Parse("#000"), StrokeWidth = 5.0f, IsStroke = true)

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
