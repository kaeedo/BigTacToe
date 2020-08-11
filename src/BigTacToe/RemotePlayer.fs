namespace BigTacToe

module RemotePlayer =
    let playPosition (model: Model) = 
        async {
            let subBoard =
                model.Board.SubBoards
                |> Seq.cast<SubBoard>
                |> Seq.filter (fun sb -> sb.IsPlayable)
                |> takeRandomItem

            let tile =
                subBoard.Tiles
                |> Seq.cast<Tile>
                |> Seq.filter (fun (_, meeple) -> meeple.IsNone)
                |> takeRandomItem
                
            let (sbi, sbj) = model.Board.GetSubBoardIndex subBoard

            let (ti, tj) = model.Board.SubBoards.[sbi, sbj].GetTileIndex tile

            do! Async.Sleep 1000

            return OpponentPlayed ((sbi, sbj), (ti, tj))
        }
