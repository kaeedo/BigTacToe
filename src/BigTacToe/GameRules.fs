﻿namespace BigTacToe

open SkiaSharp

module GameRules =
    let private calculateSubBoardWinner (tiles: Tile [,]) currentPlayer =
        let checks = [ 0; 1; 2 ]

        let anyRowWon =
            checks
            |> Seq.map (fun i ->
                tiles.[i, *]
                |> Seq.forall (fun (_, meeple) -> meeple = (Some currentPlayer)))
            |> Seq.exists id

        let anyColumnWon =
            checks
            |> Seq.map (fun i ->
                tiles.[*, i]
                |> Seq.forall (fun (_, meeple) -> meeple = (Some currentPlayer)))
            |> Seq.exists id

        let diagonalOne =
            checks
            |> Seq.map (fun i -> tiles.[i, i])
            |> Seq.forall (fun (_, meeple) -> meeple = (Some currentPlayer))

        let diagonalTwo =
            Seq.map2 (fun i j -> tiles.[i, j]) checks checks
            |> Seq.rev
            |> Seq.forall (fun (_, meeple) -> meeple = (Some currentPlayer))

        if [ anyRowWon
             anyColumnWon
             diagonalOne
             diagonalTwo ]
           |> Seq.exists id then
            Some currentPlayer
        else
            None

    let maybe = MaybeBuilder()

    let togglePlayer (current: Meeple) =
        match current with
        | Meeple.Ex -> Meeple.Oh
        | Meeple.Oh -> Meeple.Ex

    let updatedBoard model (point: SKPoint) =
        maybe {
            let! touchedSubBoard =
                model.Board.SubBoards
                |> Seq.cast<SubBoard>
                |> Seq.filter (fun sb -> sb.IsPlayable && sb.Winner.IsNone)
                |> Seq.tryFind (fun sb -> sb.Rect.Contains(point))

            let! touchedSubTile =
                touchedSubBoard.Tiles
                |> Seq.cast<Tile>
                |> Seq.filter (fun (_, meeple) -> meeple.IsNone)
                |> Seq.tryFind (fun (rect, _) -> rect.Contains(point))

            let (subI, subJ) =
                let index =
                    touchedSubBoard.Tiles
                    |> Seq.cast<Tile>
                    |> Seq.findIndex (fun t -> t = touchedSubTile)

                index / 3, index % 3

            let newTiles =
                touchedSubBoard.Tiles
                |> Array2D.map (fun t ->
                    if t = touchedSubTile then
                        let (rect, _) = t
                        rect, Some model.CurrentPlayer
                    else
                        t)

            let boardWonBy =
                calculateSubBoardWinner newTiles model.CurrentPlayer

            let newBoard =
                model.Board.SubBoards
                |> Array2D.map (fun sb ->
                    if sb = touchedSubBoard then
                        { touchedSubBoard with
                              Tiles = newTiles
                              Winner = boardWonBy }
                    else
                        sb)

            let freeMove = newBoard.[subI, subJ].Winner.IsSome

            let newBoard =
                newBoard
                |> Array2D.mapi (fun i j sb ->
                    let isPlayable = freeMove || (i = subI && j = subJ)

                    { sb with IsPlayable = isPlayable })

            return newBoard
        }
