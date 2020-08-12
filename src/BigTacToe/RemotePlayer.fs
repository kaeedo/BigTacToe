namespace BigTacToe

module RemotePlayer =
    let private playOutGame model =
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

                let (sbi, sbj) = model.Board.SubBoards |> Array2D.findIndex subBoard
                
                let (ti, tj) = model.Board.SubBoards.[sbi, sbj].Tiles |> Array2D.findIndex tile

                let subBoards = GameRules.playPosition model ((sbi, sbj), (ti, tj))

                playOut <| GameRules.updateModel model subBoards

        playOut model

    let playPosition (model: Model) = 
        async {
            let playableSubBoards =
                model.Board.SubBoards
                |> Seq.cast<SubBoard>
                |> Seq.filter (fun sb -> sb.IsPlayable)
                
            let! possiblePlays =
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
                    async {
                        let (subBoard, tile) = pp
                        let (sbi, sbj) = model.Board.SubBoards |> Array2D.findIndex subBoard
                    
                        let (ti, tj) = model.Board.SubBoards.[sbi, sbj].Tiles |> Array2D.findIndex tile

                        let subBoards = GameRules.playPosition model ((sbi, sbj), (ti, tj))
                        let! bestPlay =
                            seq {
                                for _ in 0..100 do
                                    async {
                                        return playOutGame <| GameRules.updateModel model subBoards
                                    }
                            }
                            |> Async.Parallel
                            
                        let bestPlay =
                            bestPlay
                            |> Seq.countBy id
                            |> Seq.filter (fun (bw, _) -> bw = Player Meeple.Oh)
                            |> Seq.sortByDescending snd
                            |> Seq.tryHead
                            |> Option.fold (fun _ bw -> snd bw) 0

                        return (pp, bestPlay)
                    }
                )
                |> Async.Parallel

            let possiblePlay =
                possiblePlays
                |> Seq.sortByDescending snd
                |> Seq.tryHead
                |> Option.map fst

            let (subBoard, tile) = 
                match possiblePlay with
                | Some cp -> cp
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
                
            let (sbi, sbj) = model.Board.SubBoards |> Array2D.findIndex subBoard

            let (ti, tj) = model.Board.SubBoards.[sbi, sbj].Tiles |> Array2D.findIndex tile

            return OpponentPlayed ((sbi, sbj), (ti, tj))
        }
