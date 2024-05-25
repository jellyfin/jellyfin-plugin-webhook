<h1 align="center">Jellyfin Webhook Plugin</h1>
<h3 align="center">Part of the <a href="https://jellyfin.org">Jellyfin Project</a></h3>

<p align="center">
<img alt="Plugin Banner" src="https://raw.githubusercontent.com/jellyfin/jellyfin-ux/master/plugins/SVG/jellyfin-plugin-webhook.svg?sanitize=true"/>
<br/>
<br/>
<a href="https://github.com/jellyfin/jellyfin-plugin-webhook/actions?query=workflow%3A%22Test+Build+Plugin%22">
<img alt="GitHub Workflow Status" src="https://img.shields.io/github/workflow/status/jellyfin/jellyfin-plugin-webhook/Test%20Build%20Plugin.svg">
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

#### Variables:

- Every Notifier
    - ServerId
    - ServerName
    - ServerVersion
        - `$major.$minor.$build`
    - ServerUrl
        - Server url
    - NotificationType
        - The [NotificationType](Jellyfin.Plugin.Webhook/Destinations/NotificationType.cs)

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
        - Item type
    - Year
        - Item production year
    - SeriesName
        - TV series name
    - SeasonNumber
        - Series number - direct format
    - SeasonNumber00
        - Series number - padded 00
    - SeasonNumber000
        - Series number - padded 000
    - EpisodeNumber
        - Episode number - direct format
    - EpisodeNumber00
        - Episode number - padded 00
    - EpisodeNumber000 -
        - Episode number - padded 000
    - Provider_{providerId_lowercase}
        - ProviderId is lowercase.
    - RunTimeTicks
        - The media runtime, in [Ticks](https://docs.microsoft.com/en-us/dotnet/api/system.datetime.ticks)
    - RunTime
        - The media runtime, as `hh:mm:ss`
    - AspectRatio
        - Item original aspect ratio 


- Playback
    - Includes everything from `BaseItem`
    - PlaybackPositionTicks
        - The current playback position, in [Ticks](https://docs.microsoft.com/en-us/dotnet/api/system.datetime.ticks)
    - PlaybackPosition
        - The current playback position, as `hh:mm:ss`
    - MediaSourceId
        - The media source id
    - IsPaused
        - If playback is paused
    - IsAutomated
        - If notification is automated, or user triggered
    - DeviceId
        - Playback device id
    - DeviceName
        - Playback device name
    - ClientName
        - Playback client name
    - NotificationUsername
        - User playing item. Note: multiple notifications will be sent if there are multiple users in a session
    - UserId
        - The user Id
    - PlayedToCompletion
        - `true/false`, Only when `NotificationType == PlaybackStop`

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
