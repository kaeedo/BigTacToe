namespace BigTacToe.Shared

open System

module Map =
    let Keys (m: Map<'Key, 'T>) =
        Map.fold (fun keys key _ -> key :: keys) [] m

type MaybeBuilder() =
    member this.Bind(m, f) = Option.bind f m

    member this.Return(x) = Some x

    member this.ReturnFrom(x) = x

    member this.Zero() = None

    member this.Combine(a, b) =
        match a with
        | Some _ -> a
        | None -> b ()

    member this.Delay(f) = f

    member this.Run(f) = f ()

[<AutoOpen>]
module Utilities =
    let maybe = MaybeBuilder()

    let takeRandomItem l =
        l
        |> Seq.sortBy (fun _ -> Guid.NewGuid())
        |> Seq.head

    let tryTakeRandomItem l =
        l
        |> Seq.sortBy (fun _ -> Guid.NewGuid())
        |> Seq.tryHead

[<RequireQualifiedAccess>]
module Array2D =
    let inline findIndex<'a when 'a: equality> (item: 'a) (array: 'a [,]) =
        let length = array |> Array2D.length1

        if (length) = (array |> Array2D.length2) then
            let index =
                array
                |> Seq.cast<'a>
                |> Seq.findIndex (fun a -> a = item)

            index / length, index % length
        else
            raise <| ArgumentException("Array must be square")

    let inline replace<'a when 'a: equality> (itemToReplace: 'a) (newItem: 'a) (array: 'a [,]) =
        let (i, j) = array |> findIndex itemToReplace
        array.[i, j] <- newItem
        array

    let inline replaceWith<'a when 'a: equality> (itemToReplace: 'a) (buildNewItem: 'a -> 'a) (array: 'a [,]) =
        try
            let (i, j) = array |> findIndex itemToReplace
            let newItem = buildNewItem array.[i, j]
            array.[i, j] <- newItem
            array
        with _ -> array
