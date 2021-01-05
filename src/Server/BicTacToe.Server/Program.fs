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
open BigTacToe.Server

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
        app.UseSignalR(GameHub.settings) |> ignore
        app.UseGiraffe(webApp) |> ignore

    let configureAppConfiguration (context: WebHostBuilderContext) (config: IConfigurationBuilder) =
        config
            .AddJsonFile("appsettings.json", false, true)
            .AddEnvironmentVariables()
            |> ignore

    let configureServices (services: IServiceCollection) =
        services.AddSingleton<GameManager.Manager>() |> ignore
        let sp  = services.BuildServiceProvider()
        let conf = sp.GetService<IConfiguration>()

        services.Configure<ForwardedHeadersOptions>(fun (options: ForwardedHeadersOptions) ->
            options.ForwardedHeaders <- ForwardedHeaders.XForwardedFor ||| ForwardedHeaders.XForwardedProto

            options.KnownNetworks.Clear()
            options.KnownProxies.Clear()
        ) |> ignore

        services.AddGiraffe() |> ignore
        services.AddSignalR(GameHub.settings) |> ignore

    let configureLogging (builder : ILoggingBuilder) =
        let filter (l : LogLevel) = l.Equals LogLevel.Error
        builder.AddFilter(filter).AddConsole().AddDebug() |> ignore

    [<EntryPoint>]
    let main _ =

        WebHostBuilder()
            .UseKestrel()
            .UseUrls("https://0.0.0.0:5000")
            .ConfigureAppConfiguration(configureAppConfiguration)
            .Configure(Action<IApplicationBuilder> configureApp)
            .ConfigureServices(configureServices)
            .ConfigureLogging(configureLogging)
            .Build()
            .Run()

        0