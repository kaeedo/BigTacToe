namespace MasterTacToe

open SkiaSharp
open SkiaSharp.Views.Forms

[<RequireQualifiedAccess>]
module Board =
    let drawLargeBoard (args: SKPaintSurfaceEventArgs) =
        let info = args.Info
        let surface = args.Surface
        let canvas = surface.Canvas

        use paint = new SKPaint(Color = SKColor.Parse("#00F"), StrokeWidth = 10.0f, IsStroke = true)
        let width = float32 info.Width
        let height = float32 info.Height
        let thirdWidth = width / 3.0f
        let thirdHeight = height / 3.0f
        [ (SKPoint(thirdWidth, 0.0f), SKPoint(thirdWidth, height))
          (SKPoint(thirdWidth * 2.0f, 0.0f), SKPoint(thirdWidth * 2.0f, height))
          (SKPoint(0.0f, thirdHeight), SKPoint(width, thirdHeight))
          (SKPoint(0.0f, thirdHeight * 2.0f), SKPoint(width, thirdHeight * 2.0f)) ]
        |> List.iter (fun (startPoint, endPoint) ->
            canvas.DrawLine(startPoint, endPoint, paint)
        )
