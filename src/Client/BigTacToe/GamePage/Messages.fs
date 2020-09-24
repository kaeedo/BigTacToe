namespace BigTacToe.Pages

open Fabulous
open GameRules
open SkiaSharp
open Xamarin.Forms

module internal Messages =
    let private calculateSubBoardRect i j (size: SKSizeI) =
        let width = float32 size.Width
        let height = float32 size.Height

        let left = width * i
        let top = height * j
        let right = left + width
        let bottom = top + height

        (left, top, right, bottom)

    let private calculateSubTiles (parentRect: Rect) (tiles: Tile [,]) =
        let (left, top, right, bottom) = parentRect
        let skRect = SKRect(left, top, right, bottom)
        let subSize =
            SKSizeI(int <| skRect.Width / 3.0f, int <| skRect.Height / 3.0f)

        tiles
        |> Array2D.mapi (fun i j (rect, meeple) ->
            let left =
                skRect.Left + float32 (subSize.Width * i)

            let right = left + float32 subSize.Width

            let top =
                skRect.Top + float32 (subSize.Height * j)

            let bottom = top + float32 subSize.Height
            ((left, top, right, bottom), meeple)
        )

    let private setBigSize board (size: int * int) =
        let (width, height) = size
        let contrainedSize = if width > height then height else width
        let subSize = SKSizeI(contrainedSize / 3, contrainedSize / 3)

        let litteBoards =
            board.SubBoards
            |> Array2D.mapi (fun i j subBoard ->
                let (left, top, right, bottom) =
                    calculateSubBoardRect (float32 i) (float32 j) subSize
                let r: Rect = (left, top, right, bottom)
                let a = calculateSubTiles r subBoard.Tiles 
                //let rect = SKRect
                { subBoard with
                      Rect = r
                      Tiles = a})

        { board with
              Board.Size = size
              SubBoards = litteBoards }


    let update msg (model: GameModel) =
        match msg with
        //| DisplayNewGameAlert ->
        //    let alertResult =
        //        async {
        //            let! confirmation = Application.Current.MainPage.DisplayAlert("New Game", "Are you sure you want to start a new game?", "Yes", "No") |> Async.AwaitTask
        //            return NewGameAlertResult confirmation
        //        }

        //    model, Cmd.ofAsyncMsg alertResult
        //| NewGameAlertResult shouldStartNew ->
        //    if shouldStartNew
        //    then GameModel.init (), Cmd.none
        //    else model, Cmd.none
        | ResizeCanvas size ->
            let board = setBigSize model.Board (size.Width, size.Height)
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
            
                
