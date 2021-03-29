namespace BigTacToe.Shared.Tests

open Expecto
open Swensen.Unquote
open System
open BigTacToe.Shared

module UtilitiesTests =
    [<Tests>]
    let utilitiesTests = testList "Utilities Tests" [
        testList "Array2D extension tests" [
            Tests.test "Array2d find index" {
                let array = Array2D.init 3 3 (fun i j -> i.ToString(), j.ToString())
                    
                test <@ array |> Array2D.findIndex ("1", "2") = (1, 2)  @>
            }
            
            Tests.test "Array2d find index should throw when 2D array is not square" {
                let array = Array2D.init 3 2 (fun i j -> i, j)
                
                raises<ArgumentException> <@ array |> Array2D.findIndex (0, 0) @>
            }
            
            Tests.test "Array2d find index by predicate" {
                let array = Array2D.init 3 3 (fun i j -> i.ToString(), j.ToString())
                
                test <@ array |> Array2D.findIndexBy (fun ij -> ij.ToString() = "(1, 2)") = (1, 2)  @>
            }
        ]
    ]