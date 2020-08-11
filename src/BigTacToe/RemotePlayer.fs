namespace BigTacToe

module RemotePlayer =
    let private playOutGame modell =
        let rec playOut (model: Model) =
            match model.Board.Winner with
            | Some winner -> winner
            | None ->
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

                let subBoards = GameRules.playPosition model ((sbi, sbj), (ti, tj))

                playOut <| GameRules.updateModel model subBoards

        playOut modell

    let playPosition (model: Model) = 
        async {
            let playableSubBoards =
                model.Board.SubBoards
                |> Seq.cast<SubBoard>
                |> Seq.filter (fun sb -> sb.IsPlayable)

            let possiblePlays =
                playableSubBoards
                |> Seq.map (fun psb ->
                    psb.Tiles
                    |> Seq.cast<Tile>
                    |> Seq.filter (fun (_, meeple) -> meeple.IsNone)
                    |> Seq.map (fun t ->
                        psb, t
                    )
                )
                |> Seq.concat
                |> Seq.map (fun pp ->
                    let (subBoard, tile) = pp
                    let (sbi, sbj) = model.Board.GetSubBoardIndex subBoard
                    
                    let (ti, tj) = model.Board.SubBoards.[sbi, sbj].GetTileIndex tile

                    let subBoards = GameRules.playPosition model ((sbi, sbj), (ti, tj))

                    (pp, playOutGame <| GameRules.updateModel model subBoards)
                )

            let chosenPlay =
                possiblePlays
                |> Seq.filter (fun (_, boardWinner) -> 
                    boardWinner = Player (Meeple.Oh)
                )
                |> tryTakeRandomItem

            let (subBoard, tile) = 
                match chosenPlay with
                | Some cp -> fst cp
                | None -> 
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

                    subBoard, tile
                
            let (sbi, sbj) = model.Board.GetSubBoardIndex subBoard

            let (ti, tj) = model.Board.SubBoards.[sbi, sbj].GetTileIndex tile

            do! Async.Sleep 500

            return OpponentPlayed ((sbi, sbj), (ti, tj))
        }
