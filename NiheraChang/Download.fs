module Download

open System
open FSharpPlus
open FSharp.Data
open FsToolkit.ErrorHandling
open Api

let private constructSessionInput (apiData: ApiData.Root) =
    let aSession = apiData.Media.Delivery.Movie.Session

    let session =
        SessionInput.Session(
            clientInfo = SessionInput.ClientInfo(aSession.PlayerId),
            contentAuth =
                SessionInput.ContentAuth("ht2", aSession.ContentKeyTimeout, "nicovideo", aSession.ServiceUserId),
            contentId = aSession.ContentId,
            contentSrcIdSets =
                [| SessionInput.ContentSrcIdSet(
                       [| SessionInput.ContentSrcId(
                              SessionInput.SrcIdToMux(
                                  audioSrcIds = [| "archive_aac_64kbps" |],
                                  videoSrcIds = [| "archive_h264_360p_low" |]
                              )
                          ) |]
                   ) |],
            contentType = "movie",
            contentUri = "",
            keepMethod = SessionInput.KeepMethod(SessionInput.Heartbeat(aSession.HeartbeatLifetime)),
            priority = aSession.Priority,
            protocol =
                SessionInput.Protocol(
                    "http",
                    SessionInput.Parameters(
                        httpParameters =
                            SessionInput.HttpParameters(
                                SessionInput.Parameters2(
                                    hlsParameters =
                                        SessionInput.HlsParameters(
                                            useWellKnownPort = "yes",
                                            useSsl = "yes",
                                            transferPreset = "",
                                            segmentDuration = 6000
                                        )
                                )
                            )
                    )
                ),
            recipeId = aSession.RecipeId,
            sessionOperationAuth =
                SessionInput.SessionOperationAuth(
                    sessionOperationAuthBySignature =
                        SessionInput.SessionOperationAuthBySignature(
                            signature = aSession.Signature,
                            token = aSession.Token
                        )
                ),
            timingConstraint = "unlimited"
        )


    SessionInput.Root(session = session)

let private HEARTBEAT_INTERVAL = System.TimeSpan.FromSeconds(80)

module Heartbeat =
    type Error = RequestError of exn

type Heartbeat(sessionData: SessionResponse.Data) =
    let error = new Event<Heartbeat.Error>()

    interface IDisposable with
        member self.Dispose() = self.cancellationTokenSource.Cancel()

    [<CLIEvent>]
    member _.Error = error.Publish

    member val private session = HeartBeatRequest.Root(sessionData.JsonValue) with get, set
    member val private cancellationTokenSource = new Threading.CancellationTokenSource(TimeSpan.FromHours(1))

    member private self.SendFirstHeartbeat() =
        async {
            let! response =
                Http.AsyncRequest(
                    $"https://api.dmc.nico/api/sessions/{self.session.Session.Id}?_format=json&_method=PUT",
                    httpMethod = "OPTIONS",
                    headers =
                        [ "Access-control-request-method", "POST"
                          "Access-control-request-headers", "content-type"
                          "Accept", "*/*"
                          "Host", "api.dmc.nico"
                          HttpRequestHeaders.Referer(
                              "https://nicovideo.jp/watch/" + self.session.Session.RecipeId.Split("-")[1]
                          )
                          HttpRequestHeaders.Origin("https://www.nicovideo.jp")
                          HttpRequestHeaders.AcceptEncoding "gzip, deflate, br"
                          "Sec-Fetch-Mode", "cors"
                          "Sec-Fetch-Site", "cross-site"
                          "Sec-Fetch-Dest", "empty" ]
                )
                |> Async.Catch
                |> Async.map Result.ofChoice

            return response
        }


    member private self.SendHeartbeat() =
        async {
            let! response =
                Http.AsyncRequestString(
                    $"https://api.dmc.nico/api/sessions/{self.session.Session.Id}?_format=json&_method=PUT",
                    headers =
                        [ HttpRequestHeaders.ContentType HttpContentTypes.Json
                          HttpRequestHeaders.Accept HttpContentTypes.Json
                          HttpRequestHeaders.Origin "https://www.nicovideo.jp"
                          HttpRequestHeaders.Referer(
                              "https://nicovideo.jp/watch/" + self.session.Session.RecipeId.Split("-")[1]
                          ) ],
                    body = HttpRequestBody.TextRequest(sessionData.JsonValue.ToString())
                )
                |> Async.Catch
                |> Async.map Result.ofChoice

            return response
        }

    member self.Start() =
        let inner () =
            async {
                do!
                    self.SendFirstHeartbeat()
                    |> AsyncResult.ignore
                    |> AsyncResult.mapError (Heartbeat.Error.RequestError)
                    |> AsyncResult.teeError (error.Trigger)
                    |> AsyncResult.ignoreError

                while (true) do
                    do! Async.Sleep HEARTBEAT_INTERVAL
                    let! response = self.SendHeartbeat()
                    let response = response |> Result.mapError Heartbeat.Error.RequestError

                    let tryParse =
                        Result.protect SessionResponse.Parse
                        >> Result.mapError (Heartbeat.Error.RequestError)


                    result {
                        let! response = response
                        let! response = tryParse response
                        self.session <- HeartBeatRequest.Root(response.Data.JsonValue)
                    }
                    |> Result.teeError (eprintf "%A")
                    |> Result.ignoreError

            }

        let ct = self.cancellationTokenSource.Token
        Async.Start(inner (), ct)

let private fetchWatchPage id =
    HtmlDocument.AsyncLoad($"https://www.nicovideo.jp/watch/{id}")
    |> Async.Catch
    |> Async.map (Result.ofChoice)

let private getJsInitialWatchData (watchPage: HtmlDocument) =
    watchPage.CssSelect("#js-initial-watch-data") |> List.tryHead

let private getAttributeApiData (node: HtmlNode) = node.TryGetAttribute("data-api-data")
let private parseApiData str = Result.protect (ApiData.Parse) (str)

let private postSessionCreationRequest (sessionInput: SessionInput.Root) =
    Http.AsyncRequestString(
        "https://api.dmc.nico/api/sessions?_format=json",
        headers = [ HttpRequestHeaders.ContentType HttpContentTypes.Json ],
        body = HttpRequestBody.TextRequest(sessionInput.JsonValue.ToString())
    )
    |> Async.Catch
    |> Async.map (Result.ofChoice)

let private parseSessionResponse str =
    Result.protect (SessionResponse.Parse) str

/// Creates DSharpPlus.VoiceNext-compatible audio stream using ffmpeg
let private startFfmpeg (input: string) =
    let startInfo =
        Diagnostics.ProcessStartInfo(
            fileName = "ffmpeg",
            arguments = $"-y -re -i {input} -f s16le -af volume='0.1' -ar 48000 -vn pipe:1",
            RedirectStandardOutput = true,
            UseShellExecute = false
        )

    let ffmpeg = Diagnostics.Process.Start(startInfo)
    ffmpeg


type Error =
    | FailedToFetchWatchPage of exn
    | JsInitialWatchDataNotFound
    | AttributeDataApiDataNotFound
    | FailedToParseApiData of exn
    | FailedToCreateSession of exn
    | FailedToParseSessionResponse of exn

type NiconicoAudioStream(ffmpeg, heartbeat) =
    member private _.FFmpeg: Diagnostics.Process = ffmpeg
    member private _.heartbeat: Heartbeat = heartbeat
    member _.stream: System.IO.Stream = ffmpeg.StandardOutput.BaseStream

    interface IAsyncDisposable with
        member self.DisposeAsync() =
            task {
                do (self.heartbeat :> IDisposable).Dispose()
                do self.FFmpeg.Dispose()
                do! self.stream.DisposeAsync()
            }
            |> System.Threading.Tasks.ValueTask

    static member Create(id: string) =
        asyncResult {
            let! watchPage = fetchWatchPage id |> AsyncResult.mapError (Error.FailedToFetchWatchPage)

            let! apiData =
                result {
                    let! jsInitialWatchData =
                        (getJsInitialWatchData watchPage)
                        |> Option.toResultWith Error.JsInitialWatchDataNotFound

                    let! apiData =
                        getAttributeApiData jsInitialWatchData
                        |> Option.toResultWith (Error.AttributeDataApiDataNotFound)

                    return! parseApiData (apiData.Value()) |> Result.mapError (Error.FailedToParseApiData)
                }

            let sessionInput = constructSessionInput apiData

            let! sessionResponse =
                postSessionCreationRequest (sessionInput)
                |> AsyncResult.mapError (Error.FailedToCreateSession)

            let! sessionResponse =
                parseSessionResponse sessionResponse
                |> Result.mapError (Error.FailedToParseSessionResponse)


            let heartbeat = new Heartbeat(sessionResponse.Data)
            heartbeat.Start()
            let ffmpeg = startFfmpeg (sessionResponse.Data.Session.ContentUri)

            return new NiconicoAudioStream(ffmpeg, heartbeat)
        }
