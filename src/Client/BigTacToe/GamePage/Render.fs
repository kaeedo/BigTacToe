namespace BigTacToe.Pages

open BigTacToe.Shared
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
                       (shouldStroke: bool)
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

        use lineThicknessStrokePaint =
            new SKPaint(Style = SKPaintStyle.Stroke,
                        Color = SKColors.Black,
                        StrokeWidth = strokeWidth * multiplier,
                        StrokeCap = SKStrokeCap.Round)

        use strokePaint =
            new SKPaint(Style = SKPaintStyle.Stroke,
                        Color = SKColors.Black,
                        StrokeWidth = 5.0f,
                        StrokeCap = SKStrokeCap.Round,
                        IsAntialias = true)

        use fillPaint =
            new SKPaint(Color = color, Style = SKPaintStyle.Fill, IsAntialias = true, StrokeCap = SKStrokeCap.Round)

        if shouldBlur
        then fillPaint.MaskFilter <- SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5.0f)

        use path = new SKPath()
        path.MoveTo(line1.StartX, line1.StartY)
        path.LineTo(line1.EndX, line1.EndY)

        if amount >= 0.5f then
            path.MoveTo(line2.StartX, line2.StartY)
            path.LineTo(line2.EndX, line2.EndY)

        use outlinePath = new SKPath()

        lineThicknessStrokePaint.GetFillPath(path, outlinePath)
        |> ignore

        if shouldStroke && not shouldBlur
        then canvas.DrawPath(outlinePath, strokePaint)

        canvas.DrawPath(outlinePath, fillPaint)


    let private drawOh (color: SKColor)
                       (multiplier: float32)
                       (canvas: SKCanvas)
                       (rect: SKRect)
                       (amount: float32)
                       (shouldBlur: bool)
                       (shouldStroke: bool)
                       =
        let strokeWidth = 5.0f

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

        use lineThicknessStrokePaint =
            new SKPaint(Style = SKPaintStyle.Stroke,
                        Color = SKColors.Black,
                        StrokeWidth = strokeWidth * multiplier,
                        StrokeCap = SKStrokeCap.Round)

        use strokePaint =
            new SKPaint(Style = SKPaintStyle.Stroke,
                        Color = SKColors.Black,
                        StrokeWidth = 5.0f,
                        StrokeCap = SKStrokeCap.Round,
                        IsAntialias = true)

        use fillPaint =
            new SKPaint(Color = color, Style = SKPaintStyle.Fill, IsAntialias = true)

        if shouldBlur
        then fillPaint.MaskFilter <- SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5.0f)

        let amount = 359.9f * amount

        use path = new SKPath()
        path.MoveTo(rect.Left + (rect.Width / 2.0f), rect.Top)
        path.ArcTo(rect, -90.0f, -amount, false)

        use outlinePath = new SKPath()

        lineThicknessStrokePaint.GetFillPath(path, outlinePath)
        |> ignore

        if shouldStroke && not shouldBlur
        then canvas.DrawPath(outlinePath, strokePaint)

        canvas.DrawPath(outlinePath, fillPaint)

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
                then drawEx Colors.meepleEx (multiplier) canvas rect amount shouldBlur true
                else drawOh Colors.meepleOh (multiplier) canvas rect amount shouldBlur true
            | Draw -> drawGameDraw canvas rect)

    let private calculateSubBoardRect offset i j size =
        let subBoardSize = size / 3

        let left = subBoardSize * i
        let top = subBoardSize * j
        let right = left + subBoardSize
        let bottom = top + subBoardSize

        SKRect(float32 left + offset, float32 top + offset, float32 right + offset, float32 bottom + offset)

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

    let private getTileRect positionPlayed size offset =
        let (sbi, sbj), (ti, tj) = positionPlayed

        let subBoardRect =
            calculateSubBoardRect offset sbi sbj size

        let globalIndex = (ti + (sbi * 3)), (tj + (sbj * 3))

        calculateTileRect globalIndex subBoardRect

    let private drawSubBoard (args: SKPaintSurfaceEventArgs) size sb =
        use canvas = args.Surface.Canvas

        let offset =
            let difference = args.Info.Size.Width - size
            float32 difference / 2.0f

        // TODO Move the paints?
        use oddGridPaint =
            new SKPaint(Color = Colors.oddGridBackground)

        use evenGridPaint =
            new SKPaint(Color = Colors.evenGridBackground)

        let i, j = sb.Index

        let subBoardRect = calculateSubBoardRect offset i j size

        let isOddGrid = ((i + j) * (i + j)) % 2 = 0

        canvas.DrawRect(subBoardRect, (if isOddGrid then oddGridPaint else evenGridPaint))

        use smallPaint =
            new SKPaint(Color = Colors.tileBorder, StrokeWidth = smallStroke, IsStroke = true)

        if sb.Winner.IsSome then
            smallPaint.MaskFilter <- SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 1.0f)
            smallPaint.StrokeWidth <- 0.5f

        sb.Tiles
        |> Array2D.iter (fun (globalIndex, _) ->
            let tileRect =
                calculateTileRect globalIndex subBoardRect

            canvas.DrawRect(tileRect, smallPaint))

    let drawBoard (args: SKPaintSurfaceEventArgs) (board: Board) size =
        let blurred, normal =
            board.SubBoards
            |> Seq.cast<SubBoard>
            |> Seq.toList
            |> List.partition (fun sb -> board.Winner.IsSome || sb.Winner.IsSome)

        blurred |> Seq.iter (drawSubBoard args size)

        normal |> Seq.iter (drawSubBoard args size)

    let drawWinners (args: SKPaintSurfaceEventArgs) (gameModel: GameModel) runningAnimation size =
        let board = gameModel.Board
        use canvas = args.Surface.Canvas

        let offset =
            let difference = args.Info.Size.Width - size

            float32 difference / 2.0f

        board.SubBoards
        |> Seq.cast<SubBoard>
        |> Seq.filter (fun sb ->
            match runningAnimation with
            | None -> true
            | Some ra ->
                match ra.Drawing with
                | SubBoardWinner sbw -> sb <> sbw
                | GameMove gm ->
                    let (sbIndex, _) = gm.PositionPlayed
                    sb.Index <> sbIndex
                | _ -> true)
        |> Seq.iter (fun sb ->
            let (i, j) = sb.Index

            let subBoardRect = calculateSubBoardRect offset i j (size)

            let shouldBlur = board.Winner.IsSome

            drawWinner sb.Winner canvas subBoardRect 1.0f 3.0f shouldBlur)

        let isAnimatingGameWinner = runningAnimation.IsSome

        if not isAnimatingGameWinner then
            drawWinner
                board.Winner
                canvas
                (SKRect(offset, offset, float32 size + offset, float32 size + offset))
                1.0f
                5.0f
                false

    let drawMeeple (args: SKPaintSurfaceEventArgs) (gameModel: GameModel) runningAnimation size =
        use canvas = args.Surface.Canvas

        let offset =
            let difference = args.Info.Size.Width - size

            float32 difference / 2.0f

        gameModel.GameMoves
        |> Seq.filter (fun gm ->
            match runningAnimation with
            | None -> true
            | Some ra ->
                match ra.Drawing with
                | GameMove animatingGameMove -> gm <> animatingGameMove
                | _ -> true)
        |> Seq.iter (fun gameMove ->
            let tileRect =
                getTileRect gameMove.PositionPlayed size offset

            let (sbi, sbj), _ = gameMove.PositionPlayed

            let shouldDraw =
                let isLatestPlayInSubBoard =
                    ((gameModel.GameMoves |> List.head).PositionPlayed
                     |> fst) = (sbi, sbj)

                gameModel.Board.SubBoards.[sbi, sbj].Winner.IsNone
                || (runningAnimation.IsSome && isLatestPlayInSubBoard)

            let shouldBlur =
                gameModel.Board.Winner.IsSome
                || (gameModel.Board.SubBoards.[sbi, sbj].Winner.IsSome
                    && runningAnimation.IsNone)

            match shouldDraw, shouldBlur with
            | false, _ -> ()
            | true, false ->
                if gameMove.Player.Meeple = Meeple.Ex
                then drawEx Colors.meepleEx 1.0f canvas tileRect 1.0f false false
                else drawOh Colors.meepleOh 1.0f canvas tileRect 1.0f false false
            | true, true ->
                if gameMove.Player.Meeple = Meeple.Ex
                then drawEx Colors.meepleExMuted 1.0f canvas tileRect 1.0f true false
                else drawOh Colors.meepleOhMuted 1.0f canvas tileRect 1.0f true false)

    let drawHighlights (args: SKPaintSurfaceEventArgs) (gameModel: GameModel) size =
        use canvas = args.Surface.Canvas

        let color =
            (match gameModel.CurrentPlayer.Meeple with
             | Meeple.Ex -> Colors.meepleEx
             | Meeple.Oh -> Colors.meepleOh)
                .WithAlpha(255uy)

        let fillShadow =
            SKImageFilter.CreateDropShadowOnly(0.0f, 0.0f, 4.0f, 4.0f, color)

        use paint =
            new SKPaint(IsStroke = true, StrokeWidth = largeStroke, ImageFilter = fillShadow)

        let offset =
            let difference = args.Info.Size.Width - size
            float32 difference / 2.0f

        let highlightedSubBoards =
            gameModel.Board.SubBoards
            |> Seq.cast<SubBoard>
            |> Seq.filter (fun sb -> sb.IsPlayable)

        highlightedSubBoards
        |> Seq.iter (fun hsb ->
            let i, j = hsb.Index

            let subBoardRect = calculateSubBoardRect offset i j size

            canvas.DrawRect(subBoardRect, paint))


    let startAnimations (args: SKPaintSurfaceEventArgs) (fameModel: GameModel) runningAnimation size =
        use canvas = args.Surface.Canvas

        let offset =
            let difference = args.Info.Size.Width - size

            float32 difference / 2.0f


        runningAnimation
        |> Option.iter (fun drawingAnimation ->
            match drawingAnimation.Drawing with
            | GameMove gm ->
                let tileRect =
                    getTileRect gm.PositionPlayed size offset

                let multiplier = 1.0f

                match gm.Player.Meeple with
                | Meeple.Oh ->
                    drawOh Colors.meepleOh multiplier canvas tileRect drawingAnimation.AnimationPercent false false
                | Meeple.Ex ->
                    drawEx Colors.meepleEx multiplier canvas tileRect drawingAnimation.AnimationPercent false false
            | SubBoardWinner sb ->
                match sb.Winner with
                | None -> ()
                | Some (Draw) -> ()
                | Some (Participant p) ->
                    let (i, j) = sb.Index

                    let subBoardRect = calculateSubBoardRect offset i j size

                    let multiplier = 3.0f

                    match p.Meeple with
                    | Meeple.Oh ->
                        drawOh
                            Colors.meepleOh
                            multiplier
                            canvas
                            subBoardRect
                            drawingAnimation.AnimationPercent
                            false
                            true
                    | Meeple.Ex ->
                        drawEx
                            Colors.meepleEx
                            multiplier
                            canvas
                            subBoardRect
                            drawingAnimation.AnimationPercent
                            false
                            true
            | Winner boardWinner ->
                drawWinner
                    (Some boardWinner)
                    canvas
                    (SKRect(offset, offset, float32 size + offset, float32 size + offset))
                    drawingAnimation.AnimationPercent
                    5.0f
                    false)
