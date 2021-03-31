namespace BigTacToe.Pages

open System
open BigTacToe.Shared
open Elmish
open Xamarin.Forms

[<RequireQualifiedAccess>]
module internal Animations =
    let private animationFinished dispatch drawing =
        Action(fun () -> dispatch <| FinishAnimation drawing)

    let private animateOh (drawing: Drawing) dispatch =
        let parentAnimation = Animation()

        let animation =
            Animation
                ((fun f -> dispatch (AnimatePercent(drawing, float32 f))),
                 0.0,
                 1.0,
                 Easing.CubicInOut,
                 animationFinished dispatch drawing)

        parentAnimation.Add(0.0, 1.0, animation)
        parentAnimation

    let private animateEx (drawing: Drawing) dispatch =
        let parentAnimation = Animation()

        let firstLine =
            Animation((fun f -> dispatch (AnimatePercent(drawing, float32 f))), 0.0, 0.5, Easing.CubicIn)

        let secondLine =
            Animation
                ((fun f -> dispatch (AnimatePercent(drawing, float32 f))),
                 0.5,
                 1.0,
                 Easing.CubicIn,
                 animationFinished dispatch drawing)

        parentAnimation.Add(0.0, 0.5, firstLine)
        parentAnimation.Add(0.5, 1.0, secondLine)

        parentAnimation

    let create animation dispatch =
        let animationFn drawing =
            let meeple =
                match drawing with
                | GameMove gm -> Some gm.Player.Meeple
                | SubBoardWinner sb ->
                    sb.Winner
                    |> Option.map (function
                        | Participant p -> Some p.Meeple
                        | _ -> None)
                    |> Option.flatten
                | Winner (Participant p) -> Some p.Meeple
                | _ -> None

            match meeple.Value with
            | Meeple.Ex -> animateEx drawing
            | Meeple.Oh -> animateOh drawing

        let animation =
            { animation with
                  Animation = animationFn animation.Drawing dispatch }

        dispatch <| AddAnimation animation

module internal AnimationMessages =
    let handleAnimationMessage (model: ClientGameModel) message =
        match message with
        | AddAnimation drawingAnimation ->
            let name = drawingAnimation.Drawing.ToString()

            if not <| model.Canvas.Value.AnimationIsRunning(name)
            then drawingAnimation.Animation.Commit(model.Canvas.Value, name, 25u, 500u)

            { model with
                  RunningAnimation = Some drawingAnimation },
            Cmd.none,
            GameExternalMsg.NoOp
        | FinishAnimation drawing ->
            let animation, cmd =
                match drawing with
                | GameMove gm ->
                    model.GameModel.Board.SubBoards
                    |> Seq.cast<SubBoard>
                    |> Seq.filter (fun sb -> sb.Winner.IsSome)
                    |> Seq.tryFind (fun subBoard ->
                        let (sb, _) = gm.PositionPlayed
                        subBoard.Index = sb)
                    |> function
                    | None -> None, Cmd.none
                    | Some sb ->
                        let animation =
                            DrawingAnimation.init (SubBoardWinner sb)

                        Some animation, Cmd.ofSub (fun dispatch -> Animations.create animation (AnimationMessage >> dispatch))
                | SubBoardWinner _ ->
                    match model.GameModel.Board.Winner with
                    | None -> None, Cmd.none
                    | Some bw ->
                        let animation = DrawingAnimation.init (Winner bw)
                        Some animation, Cmd.ofSub (fun dispatch -> Animations.create animation (AnimationMessage >> dispatch))
                | _ -> None, Cmd.none

            { model with RunningAnimation = animation }, cmd, GameExternalMsg.NoOp
        | AnimatePercent (_, percent) ->
            let animation =
                model.RunningAnimation
                |> Option.map (fun ra -> { ra with AnimationPercent = percent })

            { model with
                  RunningAnimation = animation },
            Cmd.none,
            GameExternalMsg.NoOp
