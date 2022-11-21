module Download

open System
open FSharp.Data
open Api

let getVideoPage id =
    HtmlDocument.Load($"https://www.nicovideo.jp/watch/{id}")


let getApiData (videoPage: HtmlDocument) =
    videoPage.Descendants(fun x -> x.HasId("js-initial-watch-data"))
    |> Seq.head
    |> fun x -> x.TryGetAttribute("data-api-data")
    |> Option.get
    |> (fun a -> a.Value() |> ApiData.Parse |> (fun x -> x.Media.Delivery.Movie.Session))


let createSession (apidata: ApiData.Session) =
    let session =
        SessionInput.Session(
            clientInfo = SessionInput.ClientInfo(apidata.PlayerId),
            contentAuth = SessionInput.ContentAuth("ht2", apidata.ContentKeyTimeout, "nicovideo", apidata.ServiceUserId),
            contentId = apidata.ContentId,
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
            keepMethod = SessionInput.KeepMethod(SessionInput.Heartbeat(apidata.HeartbeatLifetime)),
            priority = apidata.Priority,
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
            recipeId = apidata.RecipeId,
            sessionOperationAuth =
                SessionInput.SessionOperationAuth(
                    sessionOperationAuthBySignature =
                        SessionInput.SessionOperationAuthBySignature(
                            signature = apidata.Signature,
                            token = apidata.Token
                        )
                ),
            timingConstraint = "unlimited"
        )


    let sessionInput = SessionInput.Root(session = session)

    let postRequest (j: SessionInput.Root) =
        Http.RequestString(
            "https://api.dmc.nico/api/sessions?_format=json",
            headers = [ HttpRequestHeaders.ContentType HttpContentTypes.Json ],
            body = HttpRequestBody.TextRequest(j.JsonValue.ToString())
        )
        |> SessionResponse.Parse

    postRequest sessionInput

let heartbeat session =
    let inner (session: HeartBeatRequest.Root) =
        async {
            do! Async.Sleep(System.TimeSpan.FromSeconds(40))

            Http.Request(
                $"https://api.dmc.nico/api/sessions/{session.Session.Id}?_format=json&_method=PUT",
                httpMethod = "OPTIONS",
                headers =
                    [ "Access-control-request-method", "POST"
                      "Access-control-request-headers", "content-type"
                      "Accept", "*/*"
                      "Host", "api.dmc.nico"
                      HttpRequestHeaders.Referer("https://nicovideo.jp/watch/" + session.Session.RecipeId.Split("-")[1])
                      HttpRequestHeaders.Origin("https://www.nicovideo.jp")
                      HttpRequestHeaders.AcceptEncoding "gzip, deflate, br"
                      "Sec-Fetch-Mode", "cors"
                      "Sec-Fetch-Site", "cross-site"
                      "Sec-Fetch-Dest", "empty" ]
            )
            |> ignore


            let mutable session = session
            use! _c = Async.OnCancel(fun () -> eprintfn "heartbeat stopped")

            while (true) do

                printfn "%s" (session.JsonValue.ToString())

                let response =
                    Http.RequestString(
                        $"https://api.dmc.nico/api/sessions/{session.Session.Id}?_format=json&_method=PUT",
                        headers =
                            [ HttpRequestHeaders.ContentType HttpContentTypes.Json
                              HttpRequestHeaders.Accept HttpContentTypes.Json
                              HttpRequestHeaders.Origin "https://www.nicovideo.jp"
                              HttpRequestHeaders.Referer(
                                  "https://nicovideo.jp/watch/" + session.Session.RecipeId.Split("-")[1]
                              ) ],
                        body = HttpRequestBody.TextRequest(session.JsonValue.ToString())
                    )
                    |> HeartBeatResponse.Parse

                eprintfn "heartbeat response: %d" response.Meta.Status
                session <- HeartBeatRequest.Root(session = HeartBeatRequest.Session(response.Data.Session.JsonValue))
                do! Async.Sleep(System.TimeSpan.FromSeconds(40))
        }

    let cts = new System.Threading.CancellationTokenSource(System.TimeSpan.FromHours(1))
    Async.Start(inner session, cts.Token)
    let stop () = cts.Cancel()
    stop

//let stopHeartbeat =
//    heartbeat (HeartBeatRequest.Root(session = HeartBeatRequest.Session(session.JsonValue)))

let downloadAudio (input: string) =
    let startInfo =
        Diagnostics.ProcessStartInfo(
            fileName = "ffmpeg",
            arguments = $"-y -re -i {input} -f s16le -af volume='0.1' -ar 48000 -vn pipe:1"
        )
    // Diagnostics.ProcessStartInfo(fileName = "ffmpeg", arguments = $"-y -re -i {m3u8Url} -f mp3 -ar 48000 foo")

    startInfo.RedirectStandardOutput <- true
    startInfo.UseShellExecute <- false
    let ffmpeg = Diagnostics.Process.Start(startInfo)
    ffmpeg.StandardOutput.BaseStream


let createStream id =
    let videoPage = getVideoPage id
    printfn "get video page"
    let apiData = getApiData (videoPage)
    printfn "get api data"
    let sessionResponse = createSession apiData
    printfn "Session created."
    printfn "Session id: %s" sessionResponse.Data.Session.Id

    let stopHeartbeat =
        heartbeat (HeartBeatRequest.Root(session = HeartBeatRequest.Session(sessionResponse.Data.Session.JsonValue)))

    let pcm = downloadAudio (sessionResponse.Data.Session.ContentUri)
    pcm, stopHeartbeat
