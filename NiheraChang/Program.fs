open System
open DSharpPlus
open DSharpPlus.VoiceNext
open DSharpPlus.CommandsNext
open DSharpPlus.SlashCommands
open Bot

let connect (discord: DiscordClient) =
    task {
        do! discord.ConnectAsync()
        do! Threading.Tasks.Task.Delay(-1)
    }

[<EntryPoint>]
let main _ =
    let token = Environment.GetEnvironmentVariable "DISCORD_TOKEN"

    if token = null then
        eprintfn "Environment variable DISCORD_TOKEN is not set."
        exit 1

    let discord =
        new DiscordClient(
            DiscordConfiguration(
                Token = token,
                TokenType = TokenType.Bot
            )
        )


    let commands =
        discord.UseCommandsNext(CommandsNextConfiguration(StringPrefixes = [ "!" ]))

    commands.RegisterCommands<MusicBot>()

    let slash = discord.UseSlashCommands()
    slash.RegisterCommands<MusicSlash>()

    discord.UseVoiceNext() |> ignore

    connect(discord).Wait()
    0
