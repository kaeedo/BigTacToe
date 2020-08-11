namespace BigTacToe

open Fabulous
open GameRules
open SkiaSharp

[<RequireQualifiedAccess>]
module Messages =
    let private calculateSubBoardRect i j (size: SKSizeI) =
        let width = float32 size.Width
        let height = float32 size.Height

        let left = width * i
        let top = height * j
        let right = left + width
        let bottom = top + height

        (left, top, right, bottom)

    let private calculateSubTiles (parentRect: SKRect) tiles =
        let subSize =
            SKSizeI(int <| parentRect.Width / 3.0f, int <| parentRect.Height / 3.0f)

        tiles
        |> Array2D.mapi (fun i j (rect, meeple) ->
            let left =
                parentRect.Left + float32 (subSize.Width * i)

            let right = left + float32 subSize.Width

            let top =
                parentRect.Top + float32 (subSize.Height * j)

            let bottom = top + float32 subSize.Height
            SKRect(left, top, right, bottom), meeple)

    let private setBigSize board (size: SKSizeI) =
        let contrainedSize = if size.Width > size.Height then size.Height else size.Width
        let subSize = SKSizeI(contrainedSize / 3, contrainedSize / 3)

        let litteBoards =
            board.SubBoards
            |> Array2D.mapi (fun i j subBoard ->
                let (left, top, right, bottom) =
                    calculateSubBoardRect (float32 i) (float32 j) subSize

                let rect = SKRect(left, top, right, bottom)
                { subBoard with
                      Rect = rect
                      Tiles = calculateSubTiles rect subBoard.Tiles })

        { board with
              Board.Size = size
              SubBoards = litteBoards }


    let update msg (model: Model) =
        match msg with
        | ResizeCanvas size ->
            let board = setBigSize model.Board size
            { model with Board = board }, Cmd.none
        | OpponentPlayed positionPlayed ->
            let subBoards = playPosition model positionPlayed
            let model = updateModel model subBoards
            model, Cmd.none
        | SKSurfaceTouched point when
            model.CurrentPlayer = Meeple.Ex && model.Board.Winner.IsNone ->
            updatedBoard model point
            |> Option.fold (fun _ b ->
                let model = updateModel model b

                let command =
                    if model.Board.Winner.IsSome 
                    then Cmd.none
                    else Cmd.ofAsyncMsg <| RemotePlayer.playPosition model

                (model, command)
            ) (model, Cmd.none)
        | _ -> (model, Cmd.none)
            
                
