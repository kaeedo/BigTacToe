namespace MasterTacToe

open SkiaSharp
open SkiaSharp.Views.Forms

[<RequireQualifiedAccess>]
module Board =
    let largeBoard (args: SKPaintSurfaceEventArgs) =
        use paint = new SKPaint(Color = SKColor.Parse("#00F"), StrokeWidth = 10.0f, IsStroke = true)
        1
