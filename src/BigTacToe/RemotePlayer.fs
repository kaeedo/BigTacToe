namespace BigTacToe

module RemotePlayer =
    let playPosition (model: Model) = 
        async {
            let (sbi, sbj) =
                let index =
                    model.Board.SubBoards
                    |> Seq.cast<SubBoard>
                    |> Seq.findIndex (fun sb -> sb.IsPlayable)
                index / 3, index % 3
            
            let (ti, tj) = 
                let index =
                    model.Board.SubBoards.[sbi, sbj].Tiles
                    |> Seq.cast<Tile>
                    |> Seq.findIndex (fun (_, meeple) -> meeple.IsNone)
                index / 3, index % 3

            do! Async.Sleep 1000

            return OpponentPlayed ((sbi, sbj), (ti, tj))
        }
