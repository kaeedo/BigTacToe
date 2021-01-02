namespace BicTacToe.Server

open System
open Microsoft.AspNetCore.Hosting
open Giraffe
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.HttpOverrides
open Microsoft.Extensions.Logging
open Fable.SignalR

module Program =
    let webApp =
        choose [
            GET >=> choose [
                routeCi "/status" >=> text ("Ok")
            ]
        ]

    let configureApp (app: IApplicationBuilder) =
        app.UseStaticFiles() |> ignore
           //.UseAuthentication()
        app.UseHttpsRedirection() |> ignore
        app.UseSignalR(GameHub.config) |> ignore
        app.UseGiraffe(webApp) |> ignore

    let configureAppConfiguration (context: WebHostBuilderContext) (config: IConfigurationBuilder) =
        config
            .AddJsonFile("appsettings.json", false, true)
            .AddEnvironmentVariables()
            |> ignore

    let configureServices (services: IServiceCollection) =
        let sp  = services.BuildServiceProvider()
        let conf = sp.GetService<IConfiguration>()

        services.Configure<ForwardedHeadersOptions>(fun (options: ForwardedHeadersOptions) ->
            options.ForwardedHeaders <- ForwardedHeaders.XForwardedFor ||| ForwardedHeaders.XForwardedProto

            options.KnownNetworks.Clear()
            options.KnownProxies.Clear()
        ) |> ignore

        services.AddGiraffe() |> ignore
        services.AddSignalR(GameHub.config) |> ignore

    let configureLogging (builder : ILoggingBuilder) =
        let filter (l : LogLevel) = l.Equals LogLevel.Error
        builder.AddFilter(filter).AddConsole().AddDebug() |> ignore


    // Find random game
    // Join speciic game


    /////////////////////////
    // Server
    /////////////////////////

    // GameSession
    //// ID
    //// Started
    //// Private or Public
    //// GameState
    //// MovesPlayed list

    // GameMove
    //// GameSessionId
    //// TimePlayedAt
    //// MoveMade (Player * [i,j])


    /////////////////////////
    // Client
    /////////////////////////

    // GameSession
    //// PlayingAs (Ex or Oh)
    //// ID
    //// GameState

    // GameMove (send/receive to server or AI session)
    //// GameSessionId
    //// MoveMade (Player * [i,j])

    [<EntryPoint>]
    let main _ =
        //let contentRoot = Directory.GetCurrentDirectory()
        //let webRoot = Path.Combine(contentRoot, "WebRoot")

        WebHostBuilder()
            .UseKestrel()
            //.UseContentRoot(contentRoot)
            //.UseWebRoot(webRoot)
            .UseUrls("https://0.0.0.0:5000")
            .ConfigureAppConfiguration(configureAppConfiguration)
            .Configure(Action<IApplicationBuilder> configureApp)
            .ConfigureServices(configureServices)
            .ConfigureLogging(configureLogging)
            .Build()
            .Run()

        0