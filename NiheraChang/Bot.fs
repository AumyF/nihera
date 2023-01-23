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



    member self.GetChannel() =
        let voiceState = self.Member.VoiceState

        if (voiceState = null) then

            None
        else
            let channel = voiceState.Channel
            Some channel

    member self.CatchError(t: TaskResult<unit, string>) =
        let reportError msg = self.RespondAsync $"Error: {msg}"

        task {
            let! result = t

            match result with
            | Error e ->
                printfn "%s" e
                do! reportError e
            | Ok _ -> printfn "Command executed successfully."
        }

open System
open System.Text.RegularExpressions

let parseId str =
    let m = Regex(@"sm\d+").Match str
    if m.Success then Some(m.ToString()) else None

type MusicSlash() =
    inherit ApplicationCommandModule()
    static member val AudioStream: Option<NiconicoAudioStream> = None with get, set

    static member val queue: Collections.Concurrent.ConcurrentDictionary<uint64, Collections.Concurrent.ConcurrentQueue<string>> =
        Collections.Concurrent.ConcurrentDictionary() with get, set

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

    [<SlashCommand(name = "enqueue", description = "Add audio to playlist")>]
    member _.Enqueue(ctx: InteractionContext, [<Option("id", "Video id to add. Example: sm9")>] id: string) : Task =
        task {
            let guildId = ctx.Guild.Id

            let queue =
                MusicSlash.queue.GetOrAdd(guildId, Collections.Concurrent.ConcurrentQueue())

            match parseId id with
            | None ->
                do! ctx.RespondAsync "動画のIDが不正です"
                return ()
            | Some id ->
                do queue.Enqueue(id)
                let! _ = ctx.RespondAsync $"{id} をプレイリストに追加しました"

                return ()
        }

    [<SlashCommand(name = "list", description = "List audio in the queue")>]
    member _.List(ctx: InteractionContext) : Task =
        task {
            try
                let queue = MusicSlash.queue[ctx.Guild.Id]

                let msg = if queue = null then "キューが空です" else String.concat "\n" queue

                do! ctx.RespondAsync msg
            with e ->
                eprintfn "%A" e

            return ()
        }

    [<SlashCommand(name = "join", description = "Join the VC and start playing")>]
    member _.Join(ctx: InteractionContext) : Task =
        task {
            try
                let connection = ctx.Client.GetVoiceNext().GetConnection(ctx.Guild)

                use! connection =
                    if connection = null then
                        let channel = ctx.GetChannel() |> Option.get

                        task {
                            do! ctx.RespondAsync $"VC {channel.Name} に接続します"
                            return! channel.ConnectAsync()
                        }
                    else
                        Task.FromResult connection


                use transmit = connection.GetTransmitSink()

                let guildId = ctx.Guild.Id
                let queue = MusicSlash.queue[guildId]

                let mutable id = null

                while queue.TryDequeue(&id) do
                    let! audioStream = NiconicoAudioStream.Create id

                    use audioStream =
                        match audioStream with
                        | Ok stream -> stream
                        | Error e -> failwith <| e.ToString()

                    let! _ = ctx.Channel.SendMessageAsync $"https://www.nicovideo.jp/watch/{id} を再生します"
                    do! audioStream.stream.CopyToAsync(transmit)

                let! _ = ctx.Channel.SendMessageAsync $"キューが空になりました"
                ()
            with e ->
                eprintfn "%A" e

            return ()
        }

    [<SlashCommand(name = "play", description = "Play audio from niconico")>]
    member _.Play(ctx: InteractionContext, [<Option("id", "Video id to play. Example: sm9")>] id: string) : Task =
        task {
            let! result =
                taskResult {
                    let! channel = ctx.GetChannel() |> Result.requireSome "ボイスチャンネルに入ってください"

                    let connection = ctx.Client.GetVoiceNext().GetConnection(ctx.Guild)

                    use! connection =
                        if connection = null then
                            task {
                                do! ctx.RespondAsync $"VC {channel.Name} に接続します"
                                return! channel.ConnectAsync()
                            }
                        else
                            Task.FromResult connection

                    let! _ = ctx.Channel.SendMessageAsync $"再生: https://www.nicovideo.jp/watch/{id}"

                    match MusicSlash.AudioStream with
                    | Some stream -> do! (stream :> System.IAsyncDisposable).DisposeAsync()
                    | None -> do! Task.FromResult(())


                    use! audioStream = NiconicoAudioStream.Create id |> AsyncResult.mapError (fun e -> e.ToString())
                    MusicSlash.AudioStream <- Some audioStream

                    let transmit = connection.GetTransmitSink()
                    do! audioStream.stream.CopyToAsync(transmit)

                    return ()
                }

            printfn "%A" result

            do!
                result
                |> Result.either (Task.FromResult) (fun e -> ctx.RespondAsync $"{e}" |> Task.ofUnit)
        }
