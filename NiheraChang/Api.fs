module Api

open FSharp.Data

type ApiData =
    JsonProvider<"""
{"media":{"delivery":{"movie":{"session":{
  "recipeId": "nicovideo-sm40352244",
  "playerId": "nicovideo-6-Sn2ZStmsZ5_1668928609188",
  "videos": [
    "archive_h264_360p",
    "archive_h264_360p_low"
  ],
  "audios": [
    "archive_aac_64kbps"
  ],
  "movies": [],
  "protocols": [
    "http",
    "hls"
  ],
  "authTypes": {
    "http": "ht2",
    "hls": "ht2"
  },
  "serviceUserId": "6-Sn2ZStmsZ5_1668928609188",
  "token": "{\"service_id\":\"nicovideo\",\"player_id\":\"nicovideo-6-Sn2ZStmsZ5_1668928609188\",\"recipe_id\":\"nicovideo-sm40352244\",\"service_user_id\":\"6-Sn2ZStmsZ5_1668928609188\",\"protocols\":[{\"name\":\"http\",\"auth_type\":\"ht2\"},{\"name\":\"hls\",\"auth_type\":\"ht2\"}],\"videos\":[\"archive_h264_360p\",\"archive_h264_360p_low\"],\"audios\":[\"archive_aac_64kbps\"],\"movies\":[],\"created_time\":1668928609000,\"expire_time\":1669015009000,\"content_ids\":[\"out1\"],\"heartbeat_lifetime\":120000,\"content_key_timeout\":600000,\"priority\":0,\"transfer_presets\":[]}",
  "signature": "8f81dc09c52c99ff8c3d2d7297e359745846ea3e98a4c447c88cd0900dac1d04",
  "contentId": "out1",
  "heartbeatLifetime": 120000,
  "contentKeyTimeout": 600000,
  "priority": 0,
  "transferPresets": [],
  "urls": [
    {
      "url": "https://api.dmc.nico/api/sessions",
      "isWellKnownPort": true,
      "isSsl": true
    }
  ]
}}}}}
""">

type SessionInput =
    JsonProvider<"""{"session":{"recipe_id":"nicovideo-sm40352244","content_id":"out1","content_type":"movie","content_src_id_sets":[{"content_src_ids":[{"src_id_to_mux":{"video_src_ids":["archive_h264_360p","archive_h264_360p_low"],"audio_src_ids":["archive_aac_64kbps"]}},{"src_id_to_mux":{"video_src_ids":["archive_h264_360p_low"],"audio_src_ids":["archive_aac_64kbps"]}}]}],"timing_constraint":"unlimited","keep_method":{"heartbeat":{"lifetime":120000}},"protocol":{"name":"http","parameters":{"http_parameters":{"parameters":{"hls_parameters":{"use_well_known_port":"yes","use_ssl":"yes","transfer_preset":"","segment_duration":6000}}}}},"content_uri":"","session_operation_auth":{"session_operation_auth_by_signature":{"token":"{\"service_id\":\"nicovideo\",\"player_id\":\"nicovideo-6-Sn2ZStmsZ5_1668928609188\",\"recipe_id\":\"nicovideo-sm40352244\",\"service_user_id\":\"6-Sn2ZStmsZ5_1668928609188\",\"protocols\":[{\"name\":\"http\",\"auth_type\":\"ht2\"},{\"name\":\"hls\",\"auth_type\":\"ht2\"}],\"videos\":[\"archive_h264_360p\",\"archive_h264_360p_low\"],\"audios\":[\"archive_aac_64kbps\"],\"movies\":[],\"created_time\":1668928609000,\"expire_time\":1669015009000,\"content_ids\":[\"out1\"],\"heartbeat_lifetime\":120000,\"content_key_timeout\":600000,\"priority\":0,\"transfer_presets\":[]}","signature":"8f81dc09c52c99ff8c3d2d7297e359745846ea3e98a4c447c88cd0900dac1d04"}},"content_auth":{"auth_type":"ht2","content_key_timeout":600000,"service_id":"nicovideo","service_user_id":"6-Sn2ZStmsZ5_1668928609188"},"client_info":{"player_id":"nicovideo-6-Sn2ZStmsZ5_1668928609188"},"priority":0}}""", InferenceMode=Runtime.StructuralInference.InferenceMode.NoInference>

type SessionResponse =
    JsonProvider<"""
    {"meta":{"status":201,"message":"created"},"data":{"session":{"id":"0g8pltksvdyl3x2kk7eh3flp76p58xsl509me049usboh5obpa-006419170a1a2425553b0102552e3531315c372b54","recipe_id":"nicovideo-sm40352244","content_id":"out1","content_src_id_sets":[{"content_src_ids":[{"src_id_to_mux":{"video_src_ids":["archive_h264_360p"],"audio_src_ids":["archive_aac_64kbps"]}},{"src_id_to_mux":{"video_src_ids":["archive_h264_360p_low"],"audio_src_ids":["archive_aac_64kbps"]}}],"allow_subset":"yes"}],"content_type":"movie","timing_constraint":"unlimited","keep_method":{"heartbeat":{"lifetime":120000,"onetime_token":"","deletion_timeout_on_no_stream":0}},"protocol":{"name":"http","parameters":{"http_parameters":{"method":"GET","parameters":{"hls_parameters":{"segment_duration":6000,"total_duration":15000,"transfer_preset":"","use_ssl":"yes","use_well_known_port":"yes","media_segment_format":"mpeg2ts","encryption":{"empty":{}},"separate_audio_stream":"no"}}}}},"play_seek_time":0,"play_speed":1.0,"play_control_range":{"max_play_speed":1.0,"min_play_speed":1.0},"content_uri":"https:\/\/vodedge639.dmc.nico\/hlsvod\/ht2_nicovideo\/nicovideo-sm40352244_f3a7466f16e36f897fbb77cbc8b93f9ef504430150a6d5c2d369a65f0738a10e\/master.m3u8?ht2_nicovideo=6-Sn2ZStmsZ5_1668928609188.0g8pltksvd_rlmxk2_27inn7nn0rpwl","session_operation_auth":{"session_operation_auth_by_signature":{"created_time":1668928609000,"expire_time":1669015009000,"token":"{\"service_id\":\"nicovideo\",\"player_id\":\"nicovideo-6-Sn2ZStmsZ5_1668928609188\",\"recipe_id\":\"nicovideo-sm40352244\",\"service_user_id\":\"6-Sn2ZStmsZ5_1668928609188\",\"protocols\":[{\"name\":\"http\",\"auth_type\":\"ht2\"},{\"name\":\"hls\",\"auth_type\":\"ht2\"}],\"videos\":[\"archive_h264_360p\",\"archive_h264_360p_low\"],\"audios\":[\"archive_aac_64kbps\"],\"movies\":[],\"created_time\":1668928609000,\"expire_time\":1669015009000,\"content_ids\":[\"out1\"],\"heartbeat_lifetime\":120000,\"content_key_timeout\":600000,\"priority\":0,\"transfer_presets\":[]}","signature":"8f81dc09c52c99ff8c3d2d7297e359745846ea3e98a4c447c88cd0900dac1d04"}},"content_auth":{"auth_type":"ht2","max_content_count":1,"content_key_timeout":600000,"service_id":"nicovideo","service_user_id":"6-Sn2ZStmsZ5_1668928609188","content_auth_info":{"method":"query","name":"ht2_nicovideo","value":"6-Sn2ZStmsZ5_1668928609188.0g8pltksvd_rlmxk2_27inn7nn0rpwl"}},"runtime_info":{"node_id":"","execution_history":[],"thumbnailer_state":[]},"client_info":{"player_id":"nicovideo-6-Sn2ZStmsZ5_1668928609188","remote_ip":"","tracking_info":""},"created_time":1668928610377,"modified_time":1668928610377,"priority":0.0,"content_route":0,"version":"1.1","content_status":"ready"}}}
    """, InferenceMode=Runtime.StructuralInference.InferenceMode.NoInference>


type HeartBeatRequest =
    JsonProvider<"""
{"session":{"id":"slrtbpus8rv9iuijffv137h7elmnpxki0ce8ss8mqkohl8usc6-086419170a1a2425553b0102552e3531315c372b54","recipe_id":"nicovideo-sm40352244","content_id":"out1","content_src_id_sets":[{"content_src_ids":[{"src_id_to_mux":{"video_src_ids":["archive_h264_360p"],"audio_src_ids":["archive_aac_64kbps"]}},{"src_id_to_mux":{"video_src_ids":["archive_h264_360p_low"],"audio_src_ids":["archive_aac_64kbps"]}}],"allow_subset":"yes"}],"content_type":"movie","timing_constraint":"unlimited","keep_method":{"heartbeat":{"lifetime":120000,"onetime_token":"","deletion_timeout_on_no_stream":0}},"protocol":{"name":"http","parameters":{"http_parameters":{"method":"GET","parameters":{"hls_parameters":{"segment_duration":6000,"total_duration":15000,"transfer_preset":"","use_ssl":"yes","use_well_known_port":"yes","media_segment_format":"mpeg2ts","encryption":{"empty":{}},"separate_audio_stream":"no"}}}}},"play_seek_time":0,"play_speed":1,"play_control_range":{"max_play_speed":1,"min_play_speed":1},"content_uri":"https://vodedge639.dmc.nico/hlsvod/ht2_nicovideo/nicovideo-sm40352244_f3a7466f16e36f897fbb77cbc8b93f9ef504430150a6d5c2d369a65f0738a10e/master.m3u8?ht2_nicovideo=6-Kk2B5gMVD3_1668952433442.slrtbpus8r_rlnfxv_bsooa9bz0m43","session_operation_auth":{"session_operation_auth_by_signature":{"created_time":1668952433000,"expire_time":1669038833000,"token":"{\"service_id\":\"nicovideo\",\"player_id\":\"nicovideo-6-Kk2B5gMVD3_1668952433442\",\"recipe_id\":\"nicovideo-sm40352244\",\"service_user_id\":\"6-Kk2B5gMVD3_1668952433442\",\"protocols\":[{\"name\":\"http\",\"auth_type\":\"ht2\"},{\"name\":\"hls\",\"auth_type\":\"ht2\"}],\"videos\":[\"archive_h264_360p\",\"archive_h264_360p_low\"],\"audios\":[\"archive_aac_64kbps\"],\"movies\":[],\"created_time\":1668952433000,\"expire_time\":1669038833000,\"content_ids\":[\"out1\"],\"heartbeat_lifetime\":120000,\"content_key_timeout\":600000,\"priority\":0,\"transfer_presets\":[]}","signature":"cf93ab4c7478b3365c53f2e2ee31562c0221d124d6ed4a7a8012866eb68b7a4d"}},"content_auth":{"auth_type":"ht2","max_content_count":1,"content_key_timeout":600000,"service_id":"nicovideo","service_user_id":"6-Kk2B5gMVD3_1668952433442","content_auth_info":{"method":"query","name":"ht2_nicovideo","value":"6-Kk2B5gMVD3_1668952433442.slrtbpus8r_rlnfxv_bsooa9bz0m43"}},"runtime_info":{"node_id":"","execution_history":[],"thumbnailer_state":[]},"client_info":{"player_id":"nicovideo-6-Kk2B5gMVD3_1668952433442","remote_ip":"","tracking_info":""},"created_time":1668952435374,"modified_time":1668952435374,"priority":0,"content_route":0,"version":"1.1","content_status":"ready"}}
""", InferenceMode=Runtime.StructuralInference.InferenceMode.NoInference>

type HeartBeatResponse =
    JsonProvider<"""
  {"meta":{"status":200,"message":"ok"},"data":{"session":{"id":"g0jzdm3hshlysz5abiobwg86aati5yq7e4288eh2rpfa2uc7jv-0a6519170a1a2425553b0102552e3531315c372b54","recipe_id":"nicovideo-sm40352244","content_id":"out1","content_src_id_sets":[{"content_src_ids":[{"src_id_to_mux":{"video_src_ids":["archive_h264_1080p"],"audio_src_ids":["archive_aac_192kbps"]}},{"src_id_to_mux":{"video_src_ids":["archive_h264_720p"],"audio_src_ids":["archive_aac_192kbps"]}},{"src_id_to_mux":{"video_src_ids":["archive_h264_480p"],"audio_src_ids":["archive_aac_192kbps"]}},{"src_id_to_mux":{"video_src_ids":["archive_h264_360p"],"audio_src_ids":["archive_aac_192kbps"]}},{"src_id_to_mux":{"video_src_ids":["archive_h264_360p_low"],"audio_src_ids":["archive_aac_192kbps"]}}],"allow_subset":"yes"}],"content_type":"movie","timing_constraint":"unlimited","keep_method":{"heartbeat":{"lifetime":120000,"onetime_token":"","deletion_timeout_on_no_stream":0}},"protocol":{"name":"http","parameters":{"http_parameters":{"method":"GET","parameters":{"hls_parameters":{"segment_duration":6000,"total_duration":15000,"transfer_preset":"","use_ssl":"yes","use_well_known_port":"yes","media_segment_format":"mpeg2ts","encryption":{"empty":{}},"separate_audio_stream":"no"}}}}},"play_seek_time":0,"play_speed":1.0,"play_control_range":{"max_play_speed":1.0,"min_play_speed":1.0},"content_uri":"https:\/\/vodedge639.dmc.nico\/hlsvod\/ht2_nicovideo\/nicovideo-sm40352244_f7c499c1911cf3c8319f26ced56eb27f94cc8e352eb3f0956cb7f2a19eb4491b\/master.m3u8?ht2_nicovideo=6-ea1UHQJIUE_1669019911674.g0jzdm3hsh_rlow09_qpfjzjl8badr","session_operation_auth":{"session_operation_auth_by_signature":{"created_time":1669019911000,"expire_time":1669106311000,"token":"{\"service_id\":\"nicovideo\",\"player_id\":\"nicovideo-6-ea1UHQJIUE_1669019911674\",\"recipe_id\":\"nicovideo-sm40352244\",\"service_user_id\":\"6-ea1UHQJIUE_1669019911674\",\"protocols\":[{\"name\":\"http\",\"auth_type\":\"ht2\"},{\"name\":\"hls\",\"auth_type\":\"ht2\"}],\"videos\":[\"archive_h264_1080p\",\"archive_h264_360p\",\"archive_h264_360p_low\",\"archive_h264_480p\",\"archive_h264_720p\"],\"audios\":[\"archive_aac_192kbps\",\"archive_aac_64kbps\"],\"movies\":[],\"created_time\":1669019911000,\"expire_time\":1669106311000,\"content_ids\":[\"out1\"],\"heartbeat_lifetime\":120000,\"content_key_timeout\":600000,\"priority\":0,\"transfer_presets\":[]}","signature":"2449a248909042b76b5e4a04a17a3a4565824c00def5ab59351df450010496cb"}},"content_auth":{"auth_type":"ht2","max_content_count":1,"content_key_timeout":600000,"service_id":"nicovideo","service_user_id":"6-ea1UHQJIUE_1669019911674","content_auth_info":{"method":"query","name":"ht2_nicovideo","value":"6-ea1UHQJIUE_1669019911674.g0jzdm3hsh_rlow09_qpfjzjl8badr"}},"runtime_info":{"node_id":"","execution_history":[],"thumbnailer_state":[]},"client_info":{"player_id":"nicovideo-6-ea1UHQJIUE_1669019911674","remote_ip":"","tracking_info":""},"created_time":1669019913191,"modified_time":1669019993258,"priority":0.0,"content_route":0,"version":"1.1","content_status":"ready"}}}
  """>
