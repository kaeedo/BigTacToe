namespace BigTacToe.Pages

open BigTacToe.Shared

module CpuPlayer =
    let private playOutGame model =
        let rec playOut (model: GameModel) =
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

    let playPosition (model: GameModel) = 
        async {
            let! possiblePlays =
                model.Board.SubBoards
                |> Seq.cast<SubBoard>
                |> Seq.filter (fun sb -> sb.IsPlayable)
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
                                for _ in 0..50 do
                                    async {
                                        return playOutGame <| GameRules.updateModel model subBoards
                                    }
                            }
                            |> Async.Parallel
                            
                        let bestPlay =
                            bestPlay
                            |> Seq.countBy id
                            |> Seq.filter (fun (bw, _) -> 
                                match bw with
                                | Participant (Player (_, Meeple.Oh)) -> true
                                | _ -> false
                            )
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
