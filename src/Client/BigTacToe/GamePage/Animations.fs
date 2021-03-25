namespace BigTacToe.Pages

open System
open BigTacToe.Shared
open Elmish
open Xamarin.Forms

[<RequireQualifiedAccess>]
module internal Animations =
    let private drawAnimation = "drawing"

    let private animationFinished dispatch =
        Action<float, bool>(fun a b -> dispatch RemoveAnimation)

    let private animateOh model (drawing: Drawing) dispatch =
        let animation =
            Animation((fun f -> dispatch (AnimatePercent(drawing, float32 f))), 0.0, 1.0, Easing.CubicInOut)

        if (not
            <| model.Canvas.Value.AnimationIsRunning(sprintf "%s%A" drawAnimation drawing)) then
            animation.Commit
                (model.Canvas.Value,
                 sprintf "%s%A" drawAnimation drawing,
                 25u,
                 500u,
                 finished = animationFinished dispatch)

    let private animateEx model (drawing: Drawing) dispatch =
        let parentAnimation = Animation()

        let firstLine =
            Animation((fun f -> dispatch (AnimatePercent(drawing, float32 f))), 0.0, 0.5, Easing.CubicIn)

        let secondLine =
            Animation((fun f -> dispatch (AnimatePercent(drawing, float32 f))), 0.5, 1.0, Easing.CubicIn)

        parentAnimation.Add(0.0, 0.5, firstLine)
        parentAnimation.Add(0.5, 1.0, secondLine)

        if (not
            <| model.Canvas.Value.AnimationIsRunning(sprintf "%s%A" drawAnimation drawing)) then
            parentAnimation.Commit
                (model.Canvas.Value,
                 sprintf "%s%A" drawAnimation drawing,
                 25u,
                 500u,
                 finished = animationFinished dispatch)

    let getAnimations model newGm gameMove =
        let tileAnimation =
            { DrawingAnimation.Drawing = GameMove gameMove
              AnimationPercent = 0.0f }

        let animationFn =
            match gameMove.Player.Meeple with
            | Meeple.Ex -> animateEx
            | Meeple.Oh -> animateOh

        let tileAnimationCommand =
            Cmd.ofSub (fun dispatch -> animationFn model (GameMove gameMove) (AnimationMessage >> dispatch))

        let didGameMoveWinSubBoard =
            newGm.Board.SubBoards
            |> Seq.cast<SubBoard>
            |> Seq.filter (fun sb -> sb.Winner.IsSome)
            |> Seq.tryFind (fun subBoard ->
                let (sb, _) = gameMove.PositionPlayed
                subBoard.Index = sb)

        let (gameWinnerAnimation, winnerAnimationCommand) =
            match newGm.Board.Winner with
            | None -> [], Cmd.none
            | Some p ->
                let animation = { DrawingAnimation.Drawing = Winner p; AnimationPercent = 0.0f }
                let winnerAnimationCommand =
                    Cmd.ofSub (fun dispatch -> animationFn model (Winner p) (AnimationMessage >> dispatch))
                [ animation ], winnerAnimationCommand

        match didGameMoveWinSubBoard with
        | None ->
            [ tileAnimation
              yield! gameWinnerAnimation
              yield! model.Animations ],
            [ tileAnimationCommand; winnerAnimationCommand ]
        | Some sb ->
            let subBoardWinnerAnimation =
                { DrawingAnimation.Drawing = SubBoardWinner sb
                  AnimationPercent = 0.0f }

            let subBoardWinnerAnimationCommand =
                Cmd.ofSub (fun dispatch -> animationFn model (SubBoardWinner sb) (AnimationMessage >> dispatch))

            [ subBoardWinnerAnimation
              tileAnimation
              yield! gameWinnerAnimation
              yield! model.Animations ],
            [ subBoardWinnerAnimationCommand; tileAnimationCommand; winnerAnimationCommand ]

module internal AnimationMessages =
    let handleAnimationMessage (model: ClientGameModel) message =
        match message with
        | RemoveAnimation ->
            let animations =
                model.Animations
                |> List.filter (fun da -> da.AnimationPercent < 1.0f)

            { model with Animations = animations }, Cmd.none, GameExternalMsg.NoOp
        | AnimatePercent (drawing, percent) ->
            let animations =
                model.Animations
                |> List.map (fun animation ->
                    if animation.Drawing = drawing then
                        { animation with
                              AnimationPercent = percent }
                    else
                        animation)

            { model with Animations = animations }, Cmd.none, GameExternalMsg.NoOp
