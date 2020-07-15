namespace BigTacToe

open Fabulous
open SkiaSharp

[<RequireQualifiedAccess>]
module Messages =
    let private calculateSubBoardRect i j (size: SKSizeI)  =
        let width = float32 size.Width
        let height = float32 size.Height

        let left = width * i
        let top = height * j
        let right = left + width
        let bottom = top + height
        
        (left, top, right, bottom)

    let private calculateSubTiles (parentRect: SKRect) tiles =
        let subSize = SKSizeI(int <| parentRect.Width / 3.0f, int <| parentRect.Height / 3.0f)

        tiles
        |> Array2D.mapi (fun i j (rect, meeple) ->
            let left = parentRect.Left + float32 (subSize.Width * i)
            let right = left + float32 subSize.Width
            let top = parentRect.Top + float32 (subSize.Height * j)
            let bottom = top + float32 subSize.Height
            SKRect(left, top, right, bottom), meeple
        )

    let private setBigSize board (size: SKSizeI) =
        let subSize = SKSizeI(size.Width / 3, size.Height / 3)
        let litteBoards = 
            board.SubBoards
            |> Array2D.mapi (fun i j subBoard ->
                let (left, top, right, bottom) = calculateSubBoardRect (float32 i) (float32 j) subSize
                let rect = SKRect(left, top, right, bottom)
                { subBoard with 
                    Rect = rect
                    Tiles = calculateSubTiles rect subBoard.Tiles }
            )
        { board with Board.Size = size; SubBoards = litteBoards }

    let update msg (model: Model) =
        match msg with
        | ResizeCanvas size -> 
            let board = setBigSize model.Board size
            { model with Board = board }, Cmd.none
        | SKSurfaceTouched point -> 
            //find board that was touched

            let touchedSubBoard =
                model.Board.SubBoards
                |> Seq.cast<SubBoard>
                |> Seq.find (fun sb ->
                    sb.Rect.Contains(point)
                )

            let touchedTile =
                touchedSubBoard.Tiles
                |> Seq.cast<Tile>
                |> Seq.find (fun (rect, _) ->
                    rect.Contains(point)
                )

            let newTiles =
                touchedSubBoard.Tiles
                |> Array2D.map (fun t ->
                    if t = touchedTile
                    then 
                        let (rect, _) = t
                        rect, Some Meeple.Player
                    else t
                )

            let newBoard =
                model.Board.SubBoards
                |> Array2D.map (fun sb ->
                    if sb = touchedSubBoard
                    then { touchedSubBoard with Tiles = newTiles }
                    else sb
                )

            { model with TouchPoint = point; Board = { model.Board with SubBoards = newBoard } }, Cmd.none

