namespace BigTacToe.Shared

open BigTacToe.Shared

module GameRules =
    let private calculateBoardWinner (tiles: Meeple option [,]) currentPlayer =
        let checks = [ 0; 1; 2 ]

        let allSameMeeple meeple = meeple = (Some currentPlayer)

        let anyRowWon =
            checks
            |> Seq.map (fun i -> tiles.[i, *] |> Seq.forall allSameMeeple)
            |> Seq.exists id

        let anyColumnWon =
            checks
            |> Seq.map (fun i -> tiles.[*, i] |> Seq.forall allSameMeeple)
            |> Seq.exists id

        let diagonalOne =
            checks
            |> Seq.map (fun i -> tiles.[i, i])
            |> Seq.forall allSameMeeple

        let diagonalTwo =
            Seq.map2 (fun i j -> tiles.[i, j]) checks (checks |> Seq.rev)
            |> Seq.forall allSameMeeple

        if [ anyRowWon
             anyColumnWon
             diagonalOne
             diagonalTwo ]
           |> Seq.exists id then
            Some(Player currentPlayer)
        else
            None

    let private newBoard model subBoard tile =
        let (sbi, sbj) = model.Board.SubBoards |> Array2D.findIndex subBoard

        let (ti, tj) = model.Board.SubBoards.[sbi, sbj].Tiles |> Array2D.findIndex tile

        let newTiles =
            subBoard.Tiles
            |> Array2D.map (fun t ->
                if t = tile then
                    let (rect, _) = t
                    (rect, Some model.CurrentPlayer)
                else
                    t)

        let boardWonBy =
            let meeples =
                newTiles |> Array2D.map (fun (_, nt) -> nt)

            let isDraw (tiles: Tile [,]) =
                tiles
                |> Seq.cast<Tile>
                |> Seq.forall (fun (_, meeple) -> meeple.IsSome)

            match calculateBoardWinner meeples model.CurrentPlayer with
            | Some winner -> Some winner
            | None -> if isDraw newTiles then Some Draw else None
        let newBoard =
            model.Board.SubBoards
            |> Array2D.map (fun sb ->
                if sb = subBoard then
                    { subBoard with
                          Tiles = newTiles
                          Winner = boardWonBy }
                else
                    sb)

        let freeMove = newBoard.[ti, tj].Winner.IsSome

        newBoard
        |> Array2D.mapi (fun i j sb ->
            let isPlayable = sb.Winner.IsNone && (freeMove || (i = ti && j = tj))

            { sb with IsPlayable = isPlayable })

    let private togglePlayer (current: Meeple) =
        match current with
        | Meeple.Ex -> Meeple.Oh
        | Meeple.Oh -> Meeple.Ex

    let playPosition (model: GameModel) (positionPlayed: PositionPlayed) =
        let (sbi, sbj), (ti, tj) = positionPlayed
        let subBoard = model.Board.SubBoards.[sbi, sbj]
        let tile = subBoard.Tiles.[ti, tj]

        newBoard model subBoard tile

    let updatedBoard model (point: Point) =
        maybe {
            let! touchedSubBoard =
                model.Board.SubBoards
                |> Seq.cast<SubBoard>
                |> Seq.filter (fun sb -> sb.IsPlayable && sb.Winner.IsNone)
                |> Seq.tryFind (fun sb -> 
                    sb.Rect <* point
                )

            let! touchedSubTile =
                touchedSubBoard.Tiles
                |> Seq.cast<Tile>
                |> Seq.filter (fun (_, meeple) -> meeple.IsNone)
                |> Seq.tryFind (fun (rect, _) -> 
                    rect <* point
                )

            return newBoard model touchedSubBoard touchedSubTile
        }

    let private calculateGameWinner (subBoard: SubBoard [,]) currentPlayer =
        let meeples =
            subBoard
            |> Array2D.map (fun sb ->
                sb.Winner
                |> Option.bind (function
                    | Player m -> Some m
                    | _ -> None))

        let isDraw (subBoards: SubBoard [,]) =
            subBoards
            |> Seq.cast<SubBoard>
            |> Seq.forall (fun sb -> not sb.IsPlayable)

        match calculateBoardWinner meeples currentPlayer with
        | Some winner -> Some winner
        | None -> if isDraw subBoard then Some Draw else None

    let updateModel model subBoards =
        { model with
            Board =
                { model.Board with
                    SubBoards = subBoards
                    Winner = calculateGameWinner subBoards model.CurrentPlayer }
            CurrentPlayer = togglePlayer model.CurrentPlayer }