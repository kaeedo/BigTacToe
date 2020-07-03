namespace BigTacToe

open Fabulous
open SkiaSharp

[<RequireQualifiedAccess>]
module Messages =
    let calculateSubBoardRect i j (size: SKSizeI) =
        let width = float32 size.Width
        let height = float32 size.Height

        let left = width * i
        let top = height * j
        let right = left + width
        let bottom = top + height
        
        SKRect(left, top, right, bottom)

    let calculateSubTiles (rect: SKRect) (rectIndex: int * int) (tiles: (SKRect * (Meeple option)) [,]) =
        let subSize = SKSizeI(int (rect.Width / 3.0f), int (rect.Height / 3.0f))
        let (ri, rj) = rectIndex
        tiles
        |> Array2D.mapi (fun i j (r, m) ->
            let mutable rect = calculateSubBoardRect (float32 i) (float32 j) subSize
            rect.Left <- rect.Left + (rect.Width * float32 ri)
            rect.Top <- rect.Top + (rect.Height * float32 rj)
            rect, m
        )

    let setBigSize board (size: SKSizeI) =
        let subSize = SKSizeI(size.Width / 3, size.Height / 3)
        let litteBoards = 
            board.SubBoards
            |> Array2D.mapi (fun i j subBoard ->
                let rect = calculateSubBoardRect (float32 i) (float32 j) subSize
                { subBoard with 
                    Rect = rect
                    Tiles = calculateSubTiles rect (i, j) subBoard.Tiles }
            )
        { board with Board.Size = size; SubBoards = litteBoards }

    let update msg (model: Model) =
        match msg with
        | ResizeCanvas size -> 
            let board = setBigSize model.Board size
            { model with Board = board }, Cmd.none
        | SKSurfaceTouched point -> 
            { model with TouchPoint = point }, Cmd.none

