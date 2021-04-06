namespace BigTacToe.LoadTests

open System
open System.Threading.Tasks
open BicTacToe.Server
open BigTacToe.Shared
open Expecto
open Fable.SignalR
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http.Connections
open Microsoft.AspNetCore.TestHost
open NBomber.Contracts
open FSharp.Control.Tasks.V2.ContextInsensitive
open NBomber.FSharp
open System.Threading

type ServerResponse(result: obj) =
    member __.Result = result


module LoadTests =      
    let step1 (testServer: TestServer) =
        step "Player connect" {
            execute (fun ctx ->
                task {
                    let playerId = Guid.NewGuid()

                    let baseAddress = testServer.BaseAddress.ToString().TrimEnd('/')
                    use stopWaitHandle = new AutoResetEvent(false)
                    let hub: HubConnection<SignalRHub.Action, unit, unit, SignalRHub.Response, unit> =
                        SignalR.Connect<SignalRHub.Action, unit, unit, SignalRHub.Response, unit>(fun hub ->
                            hub
                                .WithUrl(sprintf "%s%s" baseAddress Endpoints.Root, (fun options ->
                                        options.HttpMessageHandlerFactory <- fun _ -> testServer.CreateHandler()
                                    ))
                                .WithAutomaticReconnect()
                                .UseMessagePack())

                    do! hub.Start() |> Async.StartAsTask
                    
                    do! hub.Send(SignalRHub.Action.OnConnect playerId)
                        |> Async.StartAsTask
                        
                    hub.OnMessage(fun m ->
                        async {
                            match m with
                            | SignalRHub.Response.Connected ->
                                ctx.Logger.Debug(sprintf "Player with ID: {%A} connected" playerId)
                                stopWaitHandle.Set() |> ignore
                        }
                        )
                    |> ignore
                    
                    stopWaitHandle.WaitOne() |> ignore
                        
                        
                    ctx.Data.["playerId"] <- playerId
                    ctx.Data.["hub"] <- hub

                    return Response.Ok()
                })
        }

    let step2 =
        step "Players join game" {
            execute (fun ctx ->
                task {
                    let playerId = ctx.Data.["playerId"] :?> Guid
                    let hub = ctx.Data.["hub"] :?> HubConnection<SignalRHub.Action, unit, unit, SignalRHub.Response, unit>
                    
                    use stopWaitHandle = new AutoResetEvent(false)
                    
                    do! hub.Send(SignalRHub.Action.SearchOrCreateGame playerId)
                        |> Async.StartAsTask

                    let mutable gameId = 0
                    hub.OnMessage(fun m ->
                        async {
                            match m with
                            | SignalRHub.Response.GameStarted (gid, _) ->
                                ctx.Logger.Debug(sprintf "Step2 received game ID {%A}" gid)
                                gameId <- gid
                                stopWaitHandle.Set() |> ignore
                        }
                        )
                    |> ignore

                    stopWaitHandle.WaitOne() |> ignore

                    return Response.Ok(gameId)
                })
        }
        
    let step3 =
        step "Play move" {
            execute (fun ctx ->
                task {
                    let gameId = ctx.GetPreviousStepResponse<GameId>()
                    
                    let playerId = ctx.Data.["playerId"] :?> Guid

                    let gameMove =
                        { GameMove.Player = {Participant.Meeple = Meeple.Ex; PlayerId = playerId}
                          PositionPlayed = (0,0), (0,0) }
                    
                    let hub = ctx.Data.["hub"] :?> HubConnection<SignalRHub.Action, unit, unit, SignalRHub.Response, unit>
                    
                    use stopWaitHandle = new AutoResetEvent(false)
                    
                    do! hub.Send(SignalRHub.Action.MakeMove (gameId, gameMove))
                        |> Async.StartAsTask
                    
                    hub.OnMessage(fun m ->
                        async {
                            match m with
                            | SignalRHub.Response.MoveMade gameMove ->
                                ctx.Logger.Debug(sprintf "Step3 received move made {%A}" gameMove)
                                stopWaitHandle.Set() |> ignore
                        }
                        )
                    |> ignore

                    stopWaitHandle.WaitOne() |> ignore
                
                    return Response.Ok()
                }
            )
        }

    let testServer =
        let getTestHost () =
              WebHostBuilder()
                .UseTestServer()
                .Configure(Action<IApplicationBuilder> Program.configureApp)
                .ConfigureServices(Program.configureServices)
                .ConfigureLogging(Program.configureLogging)
                .UseUrls("http://0.0.0.0:5000")
              
        let server = new TestServer(getTestHost ())
        server

    [<Tests>]
    let loadTests =
        testList
            "scenario"
            [ Tests.test "steps list" {
                  let scn =
                      scenario "scenario 1" {
                          load [ KeepConstant(50, seconds 30) ]
                          //warmUp (seconds 5)
                          steps [ step1 testServer; step2; step3 ]
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
              }]
