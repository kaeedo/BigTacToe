namespace BigTacToe.Pages

open BigTacToe.Shared
open SkiaSharp
open SkiaSharp
open SkiaSharp.Views.Forms

[<RequireQualifiedAccess>]
module internal Render =
    module Colors =
        /// https://paletton.com/#uid=74p1c0kkOllpJBznerGibeDfJ8a

        let largeGridDivider = SKColor.Parse("#00F")
        let oddGridBackground = SKColor.Parse("#fff")
        let evenGridBackground = SKColor.Parse("#ececec")


        let highlightLight = SKColor.Parse("#FFE832")
        let highlightBrightened = SKColor.Parse("#DDCA3D")
        let highlight = SKColor.Parse("#AA9E3C")
        let highlightMuted = SKColor.Parse("#756D32")
        let highlightDark = SKColor.Parse("#413D21")


        let tileBorder = SKColor.Parse("#3F2022")

        let meepleExLight = SKColor.Parse("#43DC2B")
        let meepleExBrightened = SKColor.Parse("#44B632")
        let meepleEx = SKColor.Parse("#3E8D31")
        let meepleExMuted = SKColor.Parse("#31602A")
        let meepleExDark = SKColor.Parse("#1F361B")

        let meepleOhLight = SKColor.Parse("#6834BE")
        let meepleOhBrightened = SKColor.Parse("#593396")
        let meepleOh = SKColor.Parse("#493074")
        let meepleOhMuted = SKColor.Parse("#36264F")
        let meepleOhDark = SKColor.Parse("#20192C")

        let gameDraw = SKColor.Parse("#00F")
        let gameWinner = SKColor.Parse("#f00")

    // #FA3140 Red + 2
    // #D73B46 Red + 1
    // #A63A42 Red
    // #723136 Red - 1
    // #3F2022 Red - 2

    let private largeStroke = 5.0f
    let private smallStroke = 2.0f

    let private drawEx (color: SKColor)
                       (multiplier: float32)
                       (canvas: SKCanvas)
                       (rect: SKRect)
                       (amount: float32)
                       (shouldBlur: bool)
                       =
        let strokeWidth = 7.0f

        let paddingHorizontal =
            rect.Width * (0.1f - (0.01f * multiplier))

        let paddingVertical =
            rect.Height * (0.1f - (0.01f * multiplier))

        let line1Percent =
            if amount < 0.5f then amount * 2.0f else 1.0f

        let line2Percent = (amount - 0.5f) * 2.0f

        let left = rect.Left + paddingHorizontal
        let top = rect.Top + paddingVertical
        let right = rect.Right - paddingHorizontal
        let bottom = rect.Bottom - paddingVertical

        let line1 =
            {| StartX = left
               StartY = top
               EndX = left + ((right - left) * line1Percent)
               EndY = top + ((bottom - top) * line1Percent) |}

        let line2 =
            {| StartX = right
               StartY = top
               EndX = right - ((right - left) * line2Percent)
               EndY = top + ((bottom - top) * line2Percent) |}
               
        use paint =
            new SKPaint(Color = color,
                        StrokeWidth = strokeWidth * multiplier,
                        IsStroke = true,
                        IsAntialias = true,
                        StrokeCap = SKStrokeCap.Round)

        if shouldBlur
        then paint.MaskFilter <- SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5.0f)

        canvas.DrawLine(line1.StartX, line1.StartY, line1.EndX, line1.EndY, paint)

        if amount >= 0.5f
        then canvas.DrawLine(line2.StartX, line2.StartY, line2.EndX, line2.EndY, paint)
        

    let private drawOh (color: SKColor)
                       (multiplier: float32)
                       (canvas: SKCanvas)
                       (rect: SKRect)
                       (amount: float32)
                       (shouldBlur: bool)
                       =
        let strokeWidth = 5.0f

        use paint =
            new SKPaint(Color = color, StrokeWidth = strokeWidth * multiplier, IsStroke = true, IsAntialias = true)

        if shouldBlur
        then paint.MaskFilter <- SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5.0f)

        let paddingHorizontal =
            rect.Width * (0.1f - (0.01f * multiplier))

        let paddingVertical =
            rect.Height * (0.1f - (0.01f * multiplier))

        let rect =
            SKRect
                (rect.Left + paddingHorizontal,
                 rect.Top + paddingVertical,
                 rect.Right - paddingHorizontal,
                 rect.Bottom - paddingVertical)

        let amount = 360.0f * amount
        canvas.DrawArc(rect, -90.0f, -amount, false, paint)

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

    let private drawWinner winner canvas rect amount multiplier shouldBlur =
        winner
        |> Option.iter (fun w ->
            match w with
            | Participant p ->
                if p.Meeple = Meeple.Ex
                then drawEx Colors.meepleEx (multiplier) canvas rect amount shouldBlur
                else drawOh Colors.meepleOh (multiplier) canvas rect amount shouldBlur
            | Draw -> drawGameDraw canvas rect)

    let private calculateSubBoardRect i j (width, height) =
        let (width, height) = width / 3, height / 3
        let left = width * i
        let top = height * j
        let right = left + width
        let bottom = top + height

        (left, top, right, bottom)
        
    let private calculateZoomedSubBoardRect i j (width, height) =
        let zoomLevel = 1.1f
        
        let (width, height) = float32 width*zoomLevel, float32 height*zoomLevel
        let (width, height) = width / 3.0f, height / 3.0f
        let left = width * float32 i
        let top = height * float32 j
        let right = left + width
        let bottom = top + height

        (int left, int top, int right, int bottom)

    let private calculateTileRect (tileI, tileJ) (subBoardRect: SKRect) =
        let iInSubBoard = tileI % 3
        let jInSubBoard = tileJ % 3

        let parentRect = subBoardRect

        let subSize =
            SKSizeI(int <| parentRect.Width / 3.0f, int <| parentRect.Height / 3.0f)

        let left =
            parentRect.Left
            + float32 (subSize.Width * iInSubBoard)

        let right = left + float32 subSize.Width

        let top =
            parentRect.Top
            + float32 (subSize.Height * jInSubBoard)

        let bottom = top + float32 subSize.Height

        SKRect(left, top, right, bottom)

    let private getTileRect positionPlayed size =
        let (sbi, sbj), (ti, tj) = positionPlayed

        let (left, top, right, bottom) = calculateSubBoardRect sbi sbj size

        let subBoardRect =
            SKRect(float32 left, float32 top, float32 right, float32 bottom)

        let globalIndex = (ti + (sbi * 3)), (tj + (sbj * 3))

        calculateTileRect globalIndex subBoardRect

    let drawBoard (args: SKPaintSurfaceEventArgs) (clientGameModel: ClientGameModel) =
        let board = clientGameModel.GameModel.Board
        use canvas = args.Surface.Canvas

        use oddGridPaint =
            new SKPaint(Color = Colors.oddGridBackground)

        use evenGridPaint =
            new SKPaint(Color = Colors.evenGridBackground)

        let blurred, normal =
            board.SubBoards
            |> Seq.cast<SubBoard>
            |> Seq.toList
            |> List.partition (fun sb -> board.Winner.IsSome || sb.Winner.IsSome)

        let zoomLevel = 1.5f
        
        let drawSubBoard sb =
            let i, j = sb.Index
            
            let subBoardRect =
                if sb.IsPlayable
                then
                    let (left, top, right, bottom) =
                        calculateSubBoardRect i j (clientGameModel.Size)

                    SKRect(float32 left, float32 top, float32 right, float32 bottom)
                else
                    let (left, top, right, bottom) =
                        calculateSubBoardRect i j clientGameModel.Size
                    SKRect(float32 left, float32 top, float32 right, float32 bottom)

            let isOddGrid = ((i + j) * (i + j)) % 2 = 0

            canvas.DrawRect(subBoardRect, (if isOddGrid then oddGridPaint else evenGridPaint))

            use smallPaint =
                new SKPaint(Color = Colors.tileBorder, StrokeWidth = smallStroke, IsStroke = true)

            if sb.Winner.IsSome
            then smallPaint.MaskFilter <- SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 1f)

            sb.Tiles
            |> Array2D.iter (fun (globalIndex, _) ->
                let tileRect =
                    calculateTileRect globalIndex subBoardRect

                canvas.DrawRect(tileRect, smallPaint))

        blurred |> Seq.iter (drawSubBoard)

        normal |> Seq.iter (drawSubBoard)

    let drawWinners (args: SKPaintSurfaceEventArgs) (clientGameModel: ClientGameModel) =
        let board = clientGameModel.GameModel.Board
        use canvas = args.Surface.Canvas

        let animatingSubBoards =
            clientGameModel.Animations
            |> Seq.filter (fun a ->
                match a.Drawing with
                | SubBoardWinner _ -> true
                | _ -> false)
            |> Seq.map (fun a ->
                let (SubBoardWinner sb) = a.Drawing
                sb)

        board.SubBoards
        |> Seq.cast<SubBoard>
        |> Seq.except animatingSubBoards
        |> Seq.iter (fun sb ->
            let (i, j) = sb.Index
            // Extract this everywhere
            let (left, top, right, bottom) =
                calculateSubBoardRect i j clientGameModel.Size

            let subBoardRect =
                SKRect(float32 left, float32 top, float32 right, float32 bottom)

            let shouldBlur = board.Winner.IsSome

            drawWinner sb.Winner canvas subBoardRect 1.0f 3.0f shouldBlur)

        let isAnimatingGameWinner =
            clientGameModel.Animations
            |> List.exists (fun a ->
                match a.Drawing with
                | Winner _ -> true
                | _ -> false)

        if not isAnimatingGameWinner then
            let (width, height) = clientGameModel.Size
            let constrainedSize = if width > height then height else width

            drawWinner
                board.Winner
                canvas
                (SKRect(0.0f, 0.0f, float32 constrainedSize, float32 constrainedSize))
                1.0f
                5.0f
                false

    let drawMeeple (args: SKPaintSurfaceEventArgs) (clientGameModel: ClientGameModel) =
        use canvas = args.Surface.Canvas

        clientGameModel.GameModel.GameMoves
        |> Seq.except
            (clientGameModel.Animations
             |> Seq.filter (fun a ->
                 match a.Drawing with
                 | GameMove _ -> true
                 | _ -> false)
             |> Seq.map (fun am ->
                 let (GameMove gm) = am.Drawing
                 gm))
        |> Seq.iter (fun gameMove ->
            let tileRect =
                getTileRect gameMove.PositionPlayed clientGameModel.Size

            let shouldBlur =
                let (sbi, sbj), _ = gameMove.PositionPlayed

                clientGameModel.GameModel.Board.Winner.IsSome
                || clientGameModel.GameModel.Board.SubBoards.[sbi, sbj]
                    .Winner.IsSome

            if gameMove.Player.Meeple = Meeple.Ex then
                drawEx
                    (if shouldBlur then Colors.meepleExMuted else Colors.meepleEx)
                    1.0f
                    canvas
                    tileRect
                    1.0f
                    shouldBlur
            else
                drawOh
                    (if shouldBlur then Colors.meepleOhMuted else Colors.meepleOh)
                    1.0f
                    canvas
                    tileRect
                    1.0f
                    shouldBlur)

    let drawHighlights (args: SKPaintSurfaceEventArgs) (clientGameModel: ClientGameModel) =
        use canvas = args.Surface.Canvas
        canvas.Scale(3.0f,3.0f, 50.0f, 50.0f)
        ()
//        use canvas = args.Surface.Canvas
//        let board = clientGameModel.GameModel.Board
//
//        use highlightPaint =
//            new SKPaint(Color = Colors.highlightLight.WithAlpha(80uy))
//
//        board.SubBoards
//        |> Array2D.iteri (fun i j sb ->
//            // Extract this everywhere
//            let (left, top, right, bottom) =
//                calculateSubBoardRect i j clientGameModel.Size
//
//            let subBoardRect =
//                SKRect(float32 left, float32 top, float32 right, float32 bottom)
//
//            if sb.IsPlayable
//            then canvas.DrawRect(subBoardRect, highlightPaint)
//
//            canvas.DrawRect
//                (subBoardRect, new SKPaint(Color = Colors.tileBorder, StrokeWidth = largeStroke, IsStroke = true)))

    let startAnimations (args: SKPaintSurfaceEventArgs) (clientGameModel: ClientGameModel) =
        use canvas = args.Surface.Canvas

        clientGameModel.Animations
        |> List.iter (fun drawingAnimation ->
            match drawingAnimation.Drawing with
            | GameMove gm ->
                let tileRect =
                    getTileRect gm.PositionPlayed clientGameModel.Size

                let multiplier = 1.0f

                match gm.Player.Meeple with
                | Meeple.Oh -> drawOh Colors.meepleOh multiplier canvas tileRect drawingAnimation.AnimationPercent false
                | Meeple.Ex -> drawEx Colors.meepleEx multiplier canvas tileRect drawingAnimation.AnimationPercent false
            | SubBoardWinner sb ->
                match sb.Winner with
                | None -> ()
                | Some (Draw) -> ()
                | Some (Participant p) ->
                    let (i, j) = sb.Index
                    // Extract this everywhere
                    let (left, top, right, bottom) =
                        calculateSubBoardRect i j clientGameModel.Size

                    let subBoardRect =
                        SKRect(float32 left, float32 top, float32 right, float32 bottom)

                    let multiplier = 3.0f

                    match p.Meeple with
                    | Meeple.Oh ->
                        drawOh Colors.meepleOh multiplier canvas subBoardRect drawingAnimation.AnimationPercent false
                    | Meeple.Ex ->
                        drawEx Colors.meepleEx multiplier canvas subBoardRect drawingAnimation.AnimationPercent false
            | Winner boardWinner ->
                let (width, height) = clientGameModel.Size
                let constrainedSize = if width > height then height else width

                drawWinner
                    (Some boardWinner)
                    canvas
                    (SKRect(0.0f, 0.0f, float32 constrainedSize, float32 constrainedSize))
                    drawingAnimation.AnimationPercent
                    5.0f
                    false)
