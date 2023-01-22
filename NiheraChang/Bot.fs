module Bot

open DSharpPlus
open DSharpPlus.SlashCommands
open DSharpPlus.Entities
open DSharpPlus.VoiceNext
open System.Threading.Tasks
open Download
open FsToolkit.ErrorHandling

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
