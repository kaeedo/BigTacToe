namespace BigTacToe.LoadTests

open System
open System.Net.WebSockets
open System.Text
open System.Threading.Tasks
open BigTacToe.Shared
open Expecto
open Fable.SignalR
open Swensen.Unquote
open NBomber.Contracts
open FSharp.Control.Tasks.V2.ContextInsensitive
open NBomber.FSharp
open System.Threading

type ServerResponse(result: obj) =
    member __.Result = result


module LoadTests =
    let step1 =
        step "Send message" {
            execute (fun ctx ->
                task {
                    let hub: HubConnection<SignalRHub.Action, unit, unit, SignalRHub.Response, unit> =
                        SignalR.Connect<SignalRHub.Action, unit, unit, SignalRHub.Response, unit>(fun hub ->
                            hub
                                .WithUrl(sprintf "http://127.0.0.1:5000%s" Endpoints.Root)
                                .WithAutomaticReconnect()
                                .UseMessagePack())
                    let playerId = Guid.NewGuid()
                    
                    do! hub.Start() |> Async.StartAsTask

                    do! hub.Send(SignalRHub.Action.OnConnect playerId)
                        |> Async.StartAsTask
                        
                    ctx.Data.["playerId"] <- playerId
                    ctx.Data.["hub"] <- hub

                    return Response.Ok(hub)
                })
        }

    let step2 =
        step "Receive response" {
            execute (fun ctx ->
                task {
                    use stopWaitHandle = new AutoResetEvent(false)

                    let playerId = ctx.Data.["playerId"] :?> Guid

                    let hub =
                        ctx.Data.["hub"] :?> HubConnection<SignalRHub.Action, unit, unit, SignalRHub.Response, unit>

                    do! hub.Send(SignalRHub.Action.SearchOrCreateGame playerId)
                        |> Async.StartAsTask

                    hub.OnMessage(fun m ->
                        async {
                            match m with
                            | SignalRHub.Response.GameStarted (gameId, _) -> test <@ gameId > 999 && gameId < 10000 @>
                            | _ -> ()
                            
                            stopWaitHandle.Set() |> ignore
                        }) |> ignore

                    stopWaitHandle.WaitOne() |> ignore
                    return Response.Ok()
                })
        }

    let sim =
        [ RampConstant(50, seconds 30)
          KeepConstant(50, seconds 30) ]

    [<Tests>]
    let loadTests =
        testList
            "scenario"
            [ Tests.test "steps list" {
                  let scn =
                      scenario "scenario 1" {
                          //load sim
                          init ignore
                          clean ignore
                          //warmUp (seconds 5)
                          steps [ step1; step2 ]
                      }

                  let suite =
                      testSuite "empty suite" {
                          report {
                              html
                              folderName "results"
                              fileName "results"
                          }

                          scenarios [ scn ]

                          runWithExitCode
                      }

                  Expect.equal suite 0 "exit code should be 0"
              } ]
