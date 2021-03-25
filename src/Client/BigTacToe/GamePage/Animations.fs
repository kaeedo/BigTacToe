namespace BigTacToe.Pages

open System
open BigTacToe.Shared
open Elmish
open Xamarin.Forms
open Xamarin.Forms

[<RequireQualifiedAccess>]
module internal Animations =
    let private drawAnimation = "drawing"

    let private animationFinished dispatch =
        Action<float, bool>(fun a b -> dispatch RemoveAnimation)

    let animateOh model (drawing: Drawing) dispatch =
        let animation =
            Animation((fun f -> dispatch (AnimatePercent(drawing, f))), 0.0, 1.0, Easing.CubicInOut)

        if (not
            <| model.Canvas.Value.AnimationIsRunning(sprintf "%s%A" drawAnimation drawing)) then
            animation.Commit
                (model.Canvas.Value,
                 sprintf "%s%A" drawAnimation drawing,
                 25u,
                 500u,
                 finished = animationFinished dispatch)

    let animateEx model (drawing: Drawing) dispatch =
        let parentAnimation = Animation()

        let firstLine =
            Animation((fun f -> dispatch (AnimatePercent(drawing, f))), 0.0, 0.5, Easing.CubicIn)

        let secondLine =
            Animation((fun f -> dispatch (AnimatePercent(drawing, f))), 0.5, 1.0, Easing.CubicIn)

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

module internal AnimationMessages =
    let handleAnimationMessage (model: ClientGameModel) message =
        match message with
        | RemoveAnimation ->
            let animations =
                model.Animations
                |> List.filter (fun da -> da.AnimationPercent >= 1.0)

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
