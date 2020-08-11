namespace BigTacToe

open System

[<AutoOpen>]
module Utilities =
    let maybe = MaybeBuilder()

    let takeRandomItem l =
        l |> Seq.sortBy (fun _ -> Guid.NewGuid()) |> Seq.head

    let tryTakeRandomItem l =
        l |> Seq.sortBy (fun _ -> Guid.NewGuid()) |> Seq.tryHead