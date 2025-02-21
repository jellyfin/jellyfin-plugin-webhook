<h1 align="center">Jellyfin Webhook Plugin</h1>
<h3 align="center">Part of the <a href="https://jellyfin.org">Jellyfin Project</a></h3>

<p align="center">
<img alt="Plugin Banner" src="https://raw.githubusercontent.com/jellyfin/jellyfin-ux/master/plugins/SVG/jellyfin-plugin-webhook.svg?sanitize=true"/>
<br/>
<br/>
<a href="https://github.com/jellyfin/jellyfin-plugin-webhook/actions?query=workflow%3A%22Test+Build+Plugin%22">
<img alt="GitHub Workflow Status" src="https://img.shields.io/github/actions/workflow/status/jellyfin/jellyfin-plugin-webhook/.github/workflows/test.yaml?branch=main">
</a>
<a href="https://github.com/jellyfin/jellyfin-plugin-webhook">
<img alt="GPLv3 License" src="https://img.shields.io/github/license/jellyfin/jellyfin-plugin-webhook.svg"/>
</a>
<a href="https://github.com/jellyfin/jellyfin-plugin-webhook/releases">
<img alt="Current Release" src="https://img.shields.io/github/release/jellyfin/jellyfin-plugin-webhook.svg"/>
</a>
</p>

## Debugging:
#### My webhook isn't working!
Change your `logging.json` file to output `debug` logs for `Jellyfin.Plugin.Webhook`. Make sure to add a comma to the end of `"System": "Warning"`
```diff
{
    "Serilog": {
        "MinimumLevel": {
            "Default": "Information",
            "Override": {
                "Microsoft": "Warning",
                "System": "Warning",
+               "Jellyfin.Plugin.Webhook": "Debug"
            }
        }

```


## Documentation:
Use [Handlebars](https://handlebarsjs.com/guide/) templating engine to format notifications however you wish.

See [Templates](Jellyfin.Plugin.Webhook/Templates) for sample templates.

#### Helpers:

- if_equals
    - if first parameter equals second parameter case insensitive
- if_exist
    - if the value of the parameter is not null or empty
- link_to
    - wrap the $url and $text in an `<a>` tag
- url_encode
    - encode the given text to url-encoded format (useful for including text with spaces in urls)

#### Variables:

- Every Notifier: Server
    - ServerId
        - Server ID
    - ServerName
        - Server name
    - ServerVersion
        - `$major.$minor.$build`
    - ServerUrl
        - Server url
    - NotificationType
        - The [NotificationType](Jellyfin.Plugin.Webhook/Destinations/NotificationType.cs)

- Every Notifier: User
    - NotificationUsername
        - Current user name
    - Username
        - Current user name
    - UserId
        - User ID
    - LastLoginDate
        - Last user login date
    - LastActivityDate
        - Last user activity date

- Every Notifier: Device
    - DeviceName
        - Playback device name
    - DeviceId
        - Playback device ID

- Every Notifier: Client
    - ClientName
        - Playback client name
    - Client
        - Playback client name
    - RemoteEndPoint 
        - IP Address of client
        
- BaseItem:
    - Timestamp
        - Current server time local
    - UtcTimestamp
        - Current server time utc
    - Name
        - Item name
    - Overview
        - Item overview
    - Tagline
        - Item tagline
    - ItemId
        - Item id
    - ItemType
        - "Movie" "Episode" "Season" "Series" "Album" "Song"
    - RunTimeTicks
        - The media runtime, in [Ticks](https://docs.microsoft.com/en-us/dotnet/api/system.datetime.ticks)
    - RunTime
        - The media runtime, as `hh:mm:ss`
    - Year
        - Item production year
    - PremiereDate
        - Item premiere year
    - Genres
        - item Genre
    - AspectRatio
        - Item aspect ratio
    - MediaSourceId
        - The media source id
    - Provider_{providerId_lowercase}
        - "Provider_tvdb" "Provider_tmdb" "Provider_imdb"

- BaseItem: Season
    - SeriesName
        - TV series name
    - Year
        - Season production year
    - SeriesId
        - Season Series ID
    - SeriesPremiereDate
        - Series premiere date
    - SeasonNumber
        - Series number - direct format
    - SeasonNumber00
        - Series number - padded 00
    - SeasonNumber000
        - Series number - padded 000

- BaseItem: Episode
    - EpisodeNumber
        - Episode number - direct format
    - EpisodeNumber00
        - Episode number - padded 00
    - EpisodeNumber000 -
        - Episode number - padded 000
    - EpisodeNumberEnd
        - Episode number end - direct format
    - EpisodeNumberEnd00
        - Episode number end - padded 00
    - EpisodeNumberEnd000
        - Episode number end - padded 000
    - AirTime
        - Episode series airtime
    - Year
        - Episode production year

- BaseItem: Audio
    - Album
        - Audio album
    - Artist
        - Audio artist
    - Year
        - Audio production year

- BaseItem: Album
    - Artist
        - Album artist
    - Year
        - Album production year

- Playback
    - PlaybackPositionTicks
        - The current playback position, in [Ticks](https://docs.microsoft.com/en-us/dotnet/api/system.datetime.ticks)
    - PlaybackPosition
        - The current playback position, as `hh:mm:ss`
    - PlayMethod
        - "Transcode" "DirectStream" "DirectPlay"
    - PlayedToCompletion
        - `true/false`, Only when `NotificationType == PlaybackStop`
    - IsPaused
        - If playback is paused
    - IsAutomated
        - If notification is automated, or user triggered
    - Likes
        - likes
    - Rating
        - rating
    - PlayCount
        - Total playcount
    - Favorite
        - Favorite
    - Played
        - Completely played
    - AudioStreamIndex
        - Audio stream index
    - SubtitleStreamIndex
        - Subtitle stream index
    - LastPlayedDate
        - Last played date

- Audio info
    - Audio_0_Title
        - Display title
    - Audio_0_Type
        - Audio type
    - Audio_0_Language
        - Audio language
    - Audio_0_Codec
        - Audio codec
    - Audio_0_Channels
        - Number of audio channels
    - Audio_0_Bitrate
        - Audio bitrate
    - Audio_0_SampleRate
        - Audio samplerate
    - Audio_0_Default
        - default audio

- Video info
    - Video_0_Title
        - Display title
    - Video_0_Type
        - Video type
    - Video_0_Codec
        - Video codec
    - Video_0_Profile
        - Video profile
    - Video_0_Level
        - Video level
    - Video_0_Height
        - Video height
    - Video_0_Width
        - Video width
    - Video_0_AspectRatio
        - Video aspect ratio
    - Video_0_Interlaced
        - Video interlaced
    - Video_0_FrameRate
        - Video framerate
    - Video_0_VideoRange
        - Video video range
    - Video_0_ColorSpace
        - Video color space
    - Video_0_ColorTransfer
        - Video color transfer
    - Video_0_ColorPrimaries
        - Video color primaries
    - Video_0_PixelFormat
        - Video pixel format
    - Video_0_RefFrames
        - Video ref frames

- Subtitle info
    - Subtitle_0_Title
        - Subtitle title
    - Subtitle_0_Type
        - Subtitle type
    - Subtitle_0_Language
        - Subtitle language
    - Subtitle_0_Codec
        - Subtitle codec
    - Subtitle_0_Default
        - Default subtitle
    - Subtitle_0_Forced
        - Forced subtitle
    - Subtitle_0_External
        - External subtitle

- Plugin info
    - PluginId
        - Plugin ID
    - PluginName
        - Plugin name
    - PluginVersion
        - Plugin version
    - PluginChangelog
        - Plugin Cchangelog
    - PluginChecksum
        - Plugin checksum
    - PluginSourceUrl
        - Plugin source URL

#### Destination Specific:

- Discord
    - MentionType
    - EmbedColor
    - AvatarUrl
    - BotUsername
- Gotify
    - Priority
- Pushbullet
- Pushover
    - Token
    - UserToken
    - Device
    - Title
    - MessageUrl
    - MessageUrlTitle
    - MessagePriority
    - NotificationSound
- SMTP
- Slack
    - BotUsername
    - SlackIconUrl

Future events can be created
from https://github.com/jellyfin/jellyfin/blob/master/Jellyfin.Server.Implementations/Events/EventingServiceCollectionExtensions.cs
