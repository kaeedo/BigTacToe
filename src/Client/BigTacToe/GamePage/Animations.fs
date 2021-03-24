namespace BigTacToe.Pages

open System
open BigTacToe.Shared
open Xamarin.Forms
open Xamarin.Forms

[<RequireQualifiedAccess>]
module Animations =
    let private drawAnimation = "drawing"

    let private animationFinished dispatch =
        Action<float, bool>(fun a b -> dispatch RemoveAnimatingMeeple)

    let animateOh model (gameMove: GameMove) dispatch =
        let animation =
            Animation((fun f -> dispatch (AnimatePercent(gameMove, f))), 0.0, 1.0, Easing.CubicInOut)

        if (not
            <| model.Canvas.Value.AnimationIsRunning(sprintf "%s%A" drawAnimation gameMove)) then
            animation.Commit
                (model.Canvas.Value,
                 sprintf "%s%A" drawAnimation gameMove,
                 25u,
                 500u,
                 finished = animationFinished dispatch)

    let animateEx model (gameMove: GameMove) dispatch =
        let parentAnimation = Animation()

        let firstLine =
            Animation((fun f -> dispatch (AnimatePercent(gameMove, f))), 0.0, 0.5, Easing.CubicIn)

        let secondLine =
            Animation((fun f -> dispatch (AnimatePercent(gameMove, f))), 0.5, 1.0, Easing.CubicIn)

        parentAnimation.Add(0.0, 0.5, firstLine)
        parentAnimation.Add(0.5, 1.0, secondLine)

        if (not
            <| model.Canvas.Value.AnimationIsRunning(sprintf "%s%A" drawAnimation gameMove)) then
            parentAnimation.Commit
                (model.Canvas.Value,
                 sprintf "%s%A" drawAnimation gameMove,
                 25u,
                 500u,
                 finished = animationFinished dispatch)
