namespace BigTacToe.Pages

open BigTacToe.Shared
open SkiaSharp
open SkiaSharp.Views.Forms

[<RequireQualifiedAccess>]
module internal Render =
    module Colors =
        let largeGridDivider = SKColor.Parse("#00F")
        let oddGridBackground = SKColor.Parse("#fff")
        let evenGridBackground = SKColor.Parse("#ececec")
        let highlight = SKColor.Parse("#ff6")
        let tileBorder = SKColor.Parse("#000")
        let meepleEx = SKColor.Parse("#F00")
        let meepleOh = SKColor.Parse("#0A0")
        let gameDraw = SKColor.Parse("#00F")
        let gameWinner = SKColor.Parse("#f00")

    let private largeStroke = 5.0f
    let private smallStroke = 2.0f

    let private drawEx (color: SKColor) (stroke: float32) (canvas: SKCanvas) (rect: SKRect) (amount: float32) =
        use paint =
            new SKPaint(Color = color, StrokeWidth = stroke, IsStroke = true)

        let paddingHorizontal = rect.Width * 0.1f
        let paddingVertical = rect.Height * 0.1f

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

        canvas.DrawLine(line1.StartX, line1.StartY, line1.EndX, line1.EndY, paint)

        if amount >= 0.5f
        then canvas.DrawLine(line2.StartX, line2.StartY, line2.EndX, line2.EndY, paint)

    let private drawOh (color: SKColor) (stroke: float32) (canvas: SKCanvas) (rect: SKRect) (amount: float32) =
        use paint =
            new SKPaint(Color = color, StrokeWidth = stroke, IsStroke = true)

        let paddingHorizontal = rect.Width * 0.1f
        let paddingVertical = rect.Height * 0.1f

        let rect =
            SKRect
                (rect.Left + paddingHorizontal,
                 rect.Top + paddingVertical,
                 rect.Right - paddingHorizontal,
                 rect.Bottom - paddingVertical)

        let amount = 360.0f * amount
        canvas.DrawArc(rect, 0.0f, amount, false, paint)

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

    let private drawWinner winner canvas rect amount multiplier =
        winner
        |> Option.iter (fun w ->
            match w with
            | Participant p ->
                if p.Meeple = Meeple.Ex
                then drawEx Colors.gameWinner (5.0f * multiplier) canvas rect amount
                else drawOh Colors.gameWinner (3.0f * multiplier) canvas rect amount
            | Draw -> drawGameDraw canvas rect)

    let private calculateSubBoardRect i j (width, height) =
        let (width, height) = width / 3, height / 3
        let left = width * i
        let top = height * j
        let right = left + width
        let bottom = top + height

        (left, top, right, bottom)

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

        use paint =
            new SKPaint(Color = Colors.largeGridDivider, StrokeWidth = largeStroke, IsStroke = true)

        use oddGridPaint =
            new SKPaint(Color = Colors.oddGridBackground)

        use evenGridPaint =
            new SKPaint(Color = Colors.evenGridBackground)

        use smallPaint =
            new SKPaint(Color = Colors.tileBorder, StrokeWidth = smallStroke, IsStroke = true)

        use transparentPaint =
            let p =
                new SKPaint(Color = Colors.highlight.WithAlpha(80uy))
            //p.MaskFilter <- SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 2.0f);
            p

        let isFreeMove =
            board.SubBoards
            |> Seq.cast<SubBoard>
            |> Seq.filter (fun sb -> sb.IsPlayable)
            |> Seq.length > 1

        board.SubBoards
        |> Array2D.iteri (fun i j sb ->
            // Extract this everywhere
            let (left, top, right, bottom) =
                calculateSubBoardRect i j clientGameModel.Size

            let subBoardRect =
                SKRect(float32 left, float32 top, float32 right, float32 bottom)

            let isOddGrid = ((i + j) * (i + j)) % 2 = 0

            // Draw sub board borders
            //canvas.Scale(2.0f)
            canvas.DrawRect(subBoardRect, (if isOddGrid then oddGridPaint else evenGridPaint))

            sb.Tiles
            |> Array2D.iter (fun (globalIndex, _) ->
                let tileRect =
                    calculateTileRect globalIndex subBoardRect

                canvas.DrawRect(tileRect, smallPaint))

            // Draw playability grey out
            if not isFreeMove && sb.IsPlayable
            then canvas.DrawRect(subBoardRect, transparentPaint))

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

            drawWinner sb.Winner canvas subBoardRect 1.0f 3.0f)

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

            if gameMove.Player.Meeple = Meeple.Ex
            then drawEx Colors.meepleEx 5.0f canvas tileRect 1.0f
            else drawOh Colors.meepleOh 3.0f canvas tileRect 1.0f)

    let startAnimations (args: SKPaintSurfaceEventArgs) (clientGameModel: ClientGameModel) =
        use canvas = args.Surface.Canvas

        clientGameModel.Animations
        |> List.iter (fun drawingAnimation ->
            match drawingAnimation.Drawing with
            | GameMove gm ->
                let tileRect =
                    getTileRect gm.PositionPlayed clientGameModel.Size

                match gm.Player.Meeple with
                | Meeple.Oh -> drawOh Colors.meepleOh 3.0f canvas tileRect drawingAnimation.AnimationPercent
                | Meeple.Ex -> drawEx Colors.meepleEx 5.0f canvas tileRect drawingAnimation.AnimationPercent
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

                    match p.Meeple with
                    | Meeple.Oh -> drawOh Colors.meepleOh 9.0f canvas subBoardRect drawingAnimation.AnimationPercent
                    | Meeple.Ex -> drawEx Colors.meepleEx 15.0f canvas subBoardRect drawingAnimation.AnimationPercent
            | Winner boardWinner ->
                let (width, height) = clientGameModel.Size
                let constrainedSize = if width > height then height else width

                drawWinner
                    (Some boardWinner)
                    canvas
                    (SKRect(0.0f, 0.0f, float32 constrainedSize, float32 constrainedSize))
                    drawingAnimation.AnimationPercent
                    5.0f)
