module Bot

open DSharpPlus.CommandsNext
open DSharpPlus.CommandsNext.Attributes
open DSharpPlus.VoiceNext
open System.Threading.Tasks
open Download

type MusicBot() =
    inherit BaseCommandModule()

    [<Command "ping">]
    [<Description "returns pong!">]
    member _.Ping(ctx: CommandContext) : Task = ctx.RespondAsync("pong!") :> Task

    [<Command "join">]
    member _.Join(ctx: CommandContext, id: string) : Task =
        task {
            try
                let voiceState = ctx.Member.VoiceState

                if (voiceState = null) then
                    let! _ = ctx.RespondAsync("えっと，まずボイスチャンネルに入ってください．")
                    return ()

                if (System.String.IsNullOrEmpty(id)) then
                    let! _ = ctx.RespondAsync("あの、動画を指定してください")
                    return ()

                let channel = voiceState.Channel

                let! _ = ctx.Client.SendMessageAsync(ctx.Channel, $"VC {channel.Name} に接続します")

                let! connection = channel.ConnectAsync()
                let! _ = ctx.Client.SendMessageAsync(ctx.Channel, $"VC {channel.Name} に接続しました．")

                let pcm, stopHeartbeat = createStream id
                let transmit = connection.GetTransmitSink()
                do! pcm.CopyToAsync(transmit)
                do! pcm.DisposeAsync()
                connection.Disconnect()
                stopHeartbeat()
            with a ->
                eprintfn "%A" a


            return ()
        }
