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

    let create drawing dispatch =
        let animationFn =
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
            | Meeple.Ex -> animateEx
            | Meeple.Oh -> animateOh

        let animation =
            { DrawingAnimation.Drawing = drawing
              AnimationPercent = 0.0f
              Animation = animationFn drawing dispatch }

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
            let cmd =
                match drawing with
                | GameMove gm ->
                    model.GameModel.Board.SubBoards
                    |> Seq.cast<SubBoard>
                    |> Seq.filter (fun sb -> sb.Winner.IsSome)
                    |> Seq.tryFind (fun subBoard ->
                        let (sb, _) = gm.PositionPlayed
                        subBoard.Index = sb)
                    |> function
                        | None -> Cmd.none
                        | Some sb ->
                            Cmd.ofSub (fun dispatch ->
                                Animations.create (SubBoardWinner sb) (AnimationMessage >> dispatch))
                | SubBoardWinner sbw ->
                    match model.GameModel.Board.Winner with
                    | None -> Cmd.none
                    | Some bw ->
                        Cmd.ofSub (fun dispatch ->
                                Animations.create (Winner bw) (AnimationMessage >> dispatch))
                | _ -> Cmd.none
            { model with RunningAnimation = None }, cmd, GameExternalMsg.NoOp
        | AnimatePercent (drawing, percent) ->
            let animation =
                model.RunningAnimation
                |> Option.map (fun ra -> { ra with AnimationPercent = percent })

            { model with
                  RunningAnimation = animation },
            Cmd.none,
            GameExternalMsg.NoOp
