namespace BigTacToe.Shared

open System

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

        if length = (array |> Array2D.length2) then
            let index =
                array
                |> Seq.cast<'a>
                |> Seq.findIndex (fun a -> a = item)

            index / length, index % length
        else
            raise <| ArgumentException("Array must be square")
            
    let inline findIndexBy<'a when 'a: equality> (predicate: 'a -> bool) (array: 'a [,]) =
        let length = array |> Array2D.length1

        if length = (array |> Array2D.length2) then
            let item =
                array
                |> Seq.cast<'a>
                |> Seq.find predicate
                
            array |> findIndex item
        else
            raise <| ArgumentException("Array must be square")