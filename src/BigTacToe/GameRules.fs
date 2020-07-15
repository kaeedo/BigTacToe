namespace BigTacToe

module GameRules =
    let maybe = MaybeBuilder()

    let togglePlayer (current: Meeple) =
        match current with
        | Meeple.Ex -> Meeple.Oh
        | Meeple.Oh -> Meeple.Ex
