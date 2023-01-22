module Bot

open DSharpPlus
open DSharpPlus.CommandsNext
open DSharpPlus.CommandsNext.Attributes
open DSharpPlus.SlashCommands
open DSharpPlus.Entities
open DSharpPlus.VoiceNext
open System.Threading.Tasks
open Download
open FsToolkit.ErrorHandling

type MusicBot() =
    inherit BaseCommandModule()

    [<Command "ping">]
    [<Description "returns pong!">]
    member _.Ping(ctx: CommandContext) : Task = ctx.RespondAsync("pong!") :> Task

    [<Command "play">]
    member _.Play(ctx: CommandContext, id: string) : Task =
        task {
            try
                let voiceState = ctx.Member.VoiceState

                if (voiceState = null) then
                    let! _ = ctx.RespondAsync("ボイスチャンネルに入ってください")
                    return ()

                let channel = voiceState.Channel

                use! connection = channel.ConnectAsync()
                let! _ = ctx.RespondAsync($"VC {channel.Name} に接続しました")

                let! audioStream = NiconicoAudioStream.Create id

                use audioStream =
                    match audioStream with
                    | Error e -> raise (System.Exception(e.ToString()))
                    | Ok s -> s

                let transmit = connection.GetTransmitSink()
                do! audioStream.stream.CopyToAsync(transmit)
            with a ->
                eprintfn "%A" a

            return ()
        }

    [<Command "join">]
    member _.Join(ctx: CommandContext, id: string) : Task =
        printfn "Join"

        taskResult {
            let voiceState = ctx.Member.VoiceState
            let! _ = ctx.RespondAsync("Hello")

            if (voiceState = null) then
                let! _ = ctx.RespondAsync("えっと，まずボイスチャンネルに入ってください．")
                return ()

            if (System.String.IsNullOrEmpty(id)) then
                let! _ = ctx.RespondAsync("あの、動画を指定してください")
                return ()

            let channel = voiceState.Channel

            let! connection = channel.ConnectAsync() |> TaskResult.ofTask
            let! _ = ctx.RespondAsync($"VC {channel.Name} に接続しました．")

            use! audioStream = NiconicoAudioStream.Create id |> Async.StartAsTask

            let transmit = connection.GetTransmitSink()
            do! audioStream.stream.CopyToAsync(transmit)

            connection.Disconnect()

            return ()
        }
        |> TaskResult.teeError (eprintfn "%A")
        |> TaskResult.ignoreError
        :> Task

type InteractionContext with

    member self.RespondAsync(msg) =
        self.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            DiscordInteractionResponseBuilder().WithContent msg
        )

    /// Join the channel.
    /// If the member which executed the command is not in any VC, return `None`
    member self.JoinVoiceChannel() =
        task {
            let voiceState = self.Member.VoiceState

            if (voiceState = null) then
                do! self.RespondAsync "ボイスチャンネルに入ってください"

                return None
            else
                let channel = voiceState.Channel

                let! connection = channel.ConnectAsync()
                let! _ = self.RespondAsync $"VC {channel.Name} に接続しました"

                return Some(connection)
        }

    member self.CatchError(t: TaskResult<unit, string>) =
        let reportError msg =
            self.RespondAsync $"Error: {msg}"

        task {
            let! result = t

            match result with
            | Error e ->
                printfn "%s" e
                do! reportError e
            | Ok _ ->
                printfn "Command executed successfully."
        }



type MusicSlash() =
    inherit ApplicationCommandModule()

    [<SlashCommand(name = "leave", description = "Leave the VC")>]
    member _.Leave(ctx: InteractionContext) : Task =
        taskResult {
            let vnext = ctx.Client.GetVoiceNext()

            let vnc = vnext.GetConnection(ctx.Guild)

            if vnc = null then
                do! ctx.RespondAsync $"VCに入っていません"
                return ()

            vnc.Disconnect()

            do! ctx.RespondAsync "切断しました"
        }
        |> ctx.CatchError
        :> Task

    [<SlashCommand(name = "play", description = "Play audio from niconico")>]
    member _.Play(ctx: InteractionContext, [<Option("id", "Video id to play. Example: sm9")>] id: string) : Task =
        task {
            let! result =
                taskResult {
                    use! connection = ctx.JoinVoiceChannel() |> Task.map (Result.requireSome "")
                    use! audioStream = NiconicoAudioStream.Create id |> AsyncResult.mapError (fun e -> e.ToString())

                    let transmit = connection.GetTransmitSink()
                    do! audioStream.stream.CopyToAsync(transmit)

                    return ()
                }

            do!
                result
                |> Result.either (Task.FromResult) (fun e -> ctx.RespondAsync $"{e}" |> Task.ofUnit)
        }
