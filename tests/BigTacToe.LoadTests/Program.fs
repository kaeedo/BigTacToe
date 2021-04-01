open System
open Expecto

[<EntryPoint>]
let main argv =
    Console.OutputEncoding <- System.Text.Encoding.UTF8
    Tests.runTestsInAssembly defaultConfig argv
