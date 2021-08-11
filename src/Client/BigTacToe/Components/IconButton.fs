namespace BigTacToe.Components

open SkiaSharp
open Xamarin.Forms
open Fabulous.XamarinForms.SkiaSharp
open Fabulous.XamarinForms.InputTypes

module internal IconButton =
    let private textColor = "#36264F"
    let private buttonColor = "#44B632"
    
    let private paths =
        [ "cpu", "M12,2A2,2 0 0,1 14,4C14,4.74 13.6,5.39 13,5.73V7H14A7,7 0 0,1 21,14H22A1,1 0 0,1 23,15V18A1,1 0 0,1 22,19H21V20A2,2 0 0,1 19,22H5A2,2 0 0,1 3,20V19H2A1,1 0 0,1 1,18V15A1,1 0 0,1 2,14H3A7,7 0 0,1 10,7H11V5.73C10.4,5.39 10,4.74 10,4A2,2 0 0,1 12,2M7.5,13A2.5,2.5 0 0,0 5,15.5A2.5,2.5 0 0,0 7.5,18A2.5,2.5 0 0,0 10,15.5A2.5,2.5 0 0,0 7.5,13M16.5,13A2.5,2.5 0 0,0 14,15.5A2.5,2.5 0 0,0 16.5,18A2.5,2.5 0 0,0 19,15.5A2.5,2.5 0 0,0 16.5,13Z"
          "matchmaking", "M15.5,14L20.5,19L19,20.5L14,15.5V14.71L13.73,14.43C12.59,15.41 11.11,16 9.5,16A6.5,6.5 0 0,1 3,9.5A6.5,6.5 0 0,1 9.5,3A6.5,6.5 0 0,1 16,9.5C16,11.11 15.41,12.59 14.43,13.73L14.71,14H15.5M9.5,4.5L8.95,4.53C8.71,5.05 8.34,5.93 8.07,7H10.93C10.66,5.93 10.29,5.05 10.05,4.53C9.87,4.5 9.69,4.5 9.5,4.5M13.83,7C13.24,5.97 12.29,5.17 11.15,4.78C11.39,5.31 11.7,6.08 11.93,7H13.83M5.17,7H7.07C7.3,6.08 7.61,5.31 7.85,4.78C6.71,5.17 5.76,5.97 5.17,7M4.5,9.5C4.5,10 4.58,10.53 4.73,11H6.87L6.75,9.5L6.87,8H4.73C4.58,8.47 4.5,9 4.5,9.5M14.27,11C14.42,10.53 14.5,10 14.5,9.5C14.5,9 14.42,8.47 14.27,8H12.13C12.21,8.5 12.25,9 12.25,9.5C12.25,10 12.21,10.5 12.13,11H14.27M7.87,8L7.75,9.5L7.87,11H11.13C11.21,10.5 11.25,10 11.25,9.5C11.25,9 11.21,8.5 11.13,8H7.87M9.5,14.5C9.68,14.5 9.86,14.5 10.03,14.47C10.28,13.95 10.66,13.07 10.93,12H8.07C8.34,13.07 8.72,13.95 8.97,14.47L9.5,14.5M13.83,12H11.93C11.7,12.92 11.39,13.69 11.15,14.22C12.29,13.83 13.24,13.03 13.83,12M5.17,12C5.76,13.03 6.71,13.83 7.85,14.22C7.61,13.69 7.3,12.92 7.07,12H5.17Z"
          "passPhone","M6 6C7.1 6 8 5.1 8 4S7.1 2 6 2 4 2.9 4 4 4.9 6 6 6M10 9.43C10 8.62 9.5 7.9 8.78 7.58C7.93 7.21 7 7 6 7S4.07 7.21 3.22 7.58C2.5 7.9 2 8.62 2 9.43V10H10V9.43M18 6C19.1 6 20 5.1 20 4S19.1 2 18 2 16 2.9 16 4 16.9 6 18 6M22 9.43C22 8.62 21.5 7.9 20.78 7.58C19.93 7.21 19 7 18 7S16.07 7.21 15.22 7.58C14.5 7.9 14 8.62 14 9.43V10H22V9.43M19 17V15L5 15V17L2 14L5 11V13L19 13V11L22 14L19 17"
          "privateGame","M11 14H9C9 9.03 13.03 5 18 5V7C14.13 7 11 10.13 11 14M18 11V9C15.24 9 13 11.24 13 14H15C15 12.34 16.34 11 18 11M7 4C7 2.89 6.11 2 5 2S3 2.89 3 4 3.89 6 5 6 7 5.11 7 4M11.45 4.5H9.45C9.21 5.92 8 7 6.5 7H3.5C2.67 7 2 7.67 2 8.5V11H8V8.74C9.86 8.15 11.25 6.5 11.45 4.5M19 17C20.11 17 21 16.11 21 15S20.11 13 19 13 17 13.89 17 15 17.89 17 19 17M20.5 18H17.5C16 18 14.79 16.92 14.55 15.5H12.55C12.75 17.5 14.14 19.15 16 19.74V22H22V19.5C22 18.67 21.33 18 20.5 18Z" ]
        // robot-angry
        // robot-dead
        // robot-happy
        // head-*
        |> dict
        
    let private canvas iconName =
        View.SKCanvasView
              (invalidate = true,
               enableTouchEvents = false,
               verticalOptions = LayoutOptions.FillAndExpand,
               horizontalOptions = LayoutOptions.FillAndExpand,
               paintSurface =
                   (fun args ->
                       let canvas = args.Surface.Canvas
                       canvas.Clear()
                       let info = args.Info
                       canvas.Translate(float32 info.Width / 2.0f, float32 info.Height / 2.0f)
                       
                       let path = SKPath.ParseSvgPathData(paths.[iconName])
                       let bounds = path.Bounds
                       let xRatio = float32 info.Width / bounds.Width - 1.0f
                       let yRatio = float32 info.Height / bounds.Height - 1.0f
                       
                       let ratio = if xRatio <= yRatio then xRatio else yRatio
                       
                       canvas.Scale(ratio)
                       canvas.Translate(-bounds.MidX, -bounds.MidY)
                       
                       canvas.DrawPath(path, new SKPaint(Color = SKColor.Parse(textColor), IsAntialias = true))
                       ))
        
    let iconButton gridProps iconName (text: string) onClick =
        Frame.frame [
            yield! gridProps
            Frame.BorderColor Color.Transparent
            Frame.BackgroundColor <| Color.FromHex(buttonColor)
            Frame.CornerRadius 5.0
            Frame.Content <|
                FlexLayout.flexLayout [
                    FlexLayout.Margin 10.0
                    FlexLayout.AlignItems FlexAlignItems.Center
                    FlexLayout.JustifyContent FlexJustify.SpaceBetween
                    FlexLayout.Children [
                        canvas iconName
                        Label.label [
                            Label.TextColor <| Color.FromHex(textColor)
                            Label.VerticalLayout LayoutOptions.Center
                            Label.FontSize <| FontSize.Size 14.0
                            Label.Text (text.ToUpperInvariant())
                        ]
                    ]
                ]
            Frame.GestureRecognizers [
                TapGestureRecognizer.tapGestureRecognizer [
                    TapGestureRecognizer.OnTapped onClick
                ]
            ]
        ]