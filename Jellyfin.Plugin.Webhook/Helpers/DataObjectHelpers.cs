using System;
using System.Collections.Generic;
using System.Globalization;
using Jellyfin.Data.Entities;
using Jellyfin.Plugin.Webhook.Destinations;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Updates;

namespace Jellyfin.Plugin.Webhook.Helpers;

/// <summary>
/// Data object helpers.
/// </summary>
public static class DataObjectHelpers
{
    /// <summary>
    /// Gets the default data object.
    /// </summary>
    /// <param name="applicationHost">Instance of the <see cref="IServerApplicationHost"/> interface.</param>
    /// <param name="notificationType">The notification type.</param>
    /// <returns>The default data object.</returns>
    public static Dictionary<string, object> GetBaseDataObject(IServerApplicationHost applicationHost, NotificationType notificationType)
    {
        var dataObject = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        dataObject["ServerId"] = applicationHost.SystemId;
        dataObject["ServerName"] = applicationHost.FriendlyName.Escape();
        dataObject["ServerVersion"] = applicationHost.ApplicationVersionString;
        dataObject["ServerUrl"] = WebhookPlugin.Instance?.Configuration.ServerUrl ?? "localhost:8096";
        dataObject[nameof(NotificationType)] = notificationType.ToString();

        return dataObject;
    }

    /// <summary>
    /// Get data object from <see cref="BaseItem"/>.
    /// </summary>
    /// <param name="dataObject">The existing data object.</param>
    /// <param name="item">Instance of the <see cref="BaseItem"/>.</param>
    /// <returns>The data object.</returns>
    public static Dictionary<string, object> AddBaseItemData(this Dictionary<string, object> dataObject, BaseItem? item)
    {
        if (item is null)
        {
            return dataObject;
        }

        dataObject["Timestamp"] = DateTime.Now;
        dataObject["UtcTimestamp"] = DateTime.UtcNow;
        dataObject["Name"] = item.Name.Escape();
        dataObject["Overview"] = item.Overview.Escape();
        dataObject["Tagline"] = item.Tagline.Escape();
        dataObject["ItemId"] = item.Id;
        dataObject["ItemType"] = item.GetType().Name.Escape();
        dataObject["RunTimeTicks"] = item.RunTimeTicks ?? 0;
        dataObject["RunTime"] = TimeSpan.FromTicks(item.RunTimeTicks ?? 0).ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);

        if (item.ProductionYear is not null)
        {
            dataObject["Year"] = item.ProductionYear;
        }

        if (item.PremiereDate is not null)
        {
            dataObject["PremiereDate"] = item.PremiereDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        if (item.Genres is not null && item.Genres.Length > 0)
        {
            dataObject["Genres"] = string.Join(", ", item.Genres);
        }

        switch (item)
        {
            case Season season:
                if (!string.IsNullOrEmpty(season.Series?.Name))
                {
                    dataObject["SeriesName"] = season.Series.Name.Escape();
                }

                if (season.Series?.ProductionYear is not null)
                {
                    dataObject["Year"] = season.Series.ProductionYear;
                }

                if (season.Series?.Id is not null)
                {
                    dataObject["SeriesId"] = season.Series.Id;
                }

                if (season.IndexNumber is not null)
                {
                    dataObject["SeasonNumber"] = season.IndexNumber;
                    dataObject["SeasonNumber00"] = season.IndexNumber.Value.ToString("00", CultureInfo.InvariantCulture);
                    dataObject["SeasonNumber000"] = season.IndexNumber.Value.ToString("000", CultureInfo.InvariantCulture);
                }

                break;
            case Episode episode:
                if (!string.IsNullOrEmpty(episode.Series?.Name))
                {
                    dataObject["SeriesName"] = episode.Series.Name.Escape();
                }

                if (episode.Series?.Id is not null)
                {
                    dataObject["SeriesId"] = episode.Series.Id;
                }

                if (!episode.SeasonId.Equals(default))
                {
                    dataObject["SeasonId"] = episode.SeasonId;
                }

                if (episode.Series?.PremiereDate is not null)
                {
                    dataObject["SeriesPremiereDate"] = episode.Series.PremiereDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }

                if (episode.Season?.IndexNumber is not null)
                {
                    dataObject["SeasonNumber"] = episode.Season.IndexNumber;
                    dataObject["SeasonNumber00"] = episode.Season.IndexNumber.Value.ToString("00", CultureInfo.InvariantCulture);
                    dataObject["SeasonNumber000"] = episode.Season.IndexNumber.Value.ToString("000", CultureInfo.InvariantCulture);
                }

                if (episode.IndexNumber is not null)
                {
                    dataObject["EpisodeNumber"] = episode.IndexNumber;
                    dataObject["EpisodeNumber00"] = episode.IndexNumber.Value.ToString("00", CultureInfo.InvariantCulture);
                    dataObject["EpisodeNumber000"] = episode.IndexNumber.Value.ToString("000", CultureInfo.InvariantCulture);
                }

                if (episode.IndexNumberEnd is not null)
                {
                    dataObject["EpisodeNumberEnd"] = episode.IndexNumberEnd;
                    dataObject["EpisodeNumberEnd00"] = episode.IndexNumberEnd.Value.ToString("00", CultureInfo.InvariantCulture);
                    dataObject["EpisodeNumberEnd000"] = episode.IndexNumberEnd.Value.ToString("000", CultureInfo.InvariantCulture);
                }

                if (episode.Series?.ProductionYear is not null)
                {
                    dataObject["Year"] = episode.Series.ProductionYear;
                }

                if (episode.Series?.AirTime is not null)
                {
                    dataObject["AirTime"] = episode.Series.AirTime;
                }

                break;
            case Audio audio:
                if (!string.IsNullOrEmpty(audio.Album))
                {
                    dataObject["Album"] = audio.Album;
                }

                if (audio.Artists.Count != 0)
                {
                    // Should all artists be sent?
                    dataObject["Artist"] = audio.Artists[0];
                }

                if (audio.ProductionYear is not null)
                {
                    dataObject["Year"] = audio.ProductionYear;
                }

                break;

            case MusicAlbum album:
                if (album.Artists.Count != 0)
                {
                    // Should all artists be sent?
                    dataObject["Artist"] = album.Artists[0];
                }

                if (album.ProductionYear is not null)
                {
                    dataObject["Year"] = album.ProductionYear;
                }

                break;
        }

        foreach (var (providerKey, providerValue) in item.ProviderIds)
        {
            dataObject[$"Provider_{providerKey.ToLowerInvariant()}"] = providerValue;
        }

        var itemMediaStreams = item.GetMediaStreams();
        if (itemMediaStreams is not null)
        {
            var streamCounter = new Dictionary<MediaStreamType, int>();
            foreach (var mediaStream in itemMediaStreams)
            {
                streamCounter.TryGetValue(mediaStream.Type, out var count);
                streamCounter[mediaStream.Type] = count + 1;
                var baseKey = $"{mediaStream.Type}_{count}";

                switch (mediaStream.Type)
                {
                    case MediaStreamType.Audio:
                    {
                        dataObject[baseKey + "_Title"] = mediaStream.DisplayTitle;
                        dataObject[baseKey + "_Type"] = mediaStream.Type.ToString();
                        dataObject[baseKey + "_Language"] = mediaStream.Language;
                        dataObject[baseKey + "_Codec"] = mediaStream.Codec;
                        dataObject[baseKey + "_Channels"] = mediaStream.Channels ?? 0;
                        dataObject[baseKey + "_Bitrate"] = mediaStream.BitRate ?? 0;
                        dataObject[baseKey + "_SampleRate"] = mediaStream.SampleRate ?? 0;
                        dataObject[baseKey + "_Default"] = mediaStream.IsDefault;
                        break;
                    }

                    case MediaStreamType.Video:
                    {
                        dataObject[baseKey + "_Title"] = mediaStream.DisplayTitle;
                        dataObject[baseKey + "_Type"] = mediaStream.Type.ToString();
                        dataObject[baseKey + "_Codec"] = mediaStream.Codec;
                        dataObject[baseKey + "_Profile"] = mediaStream.Profile;
                        dataObject[baseKey + "_Level"] = mediaStream.Level ?? 0;
                        dataObject[baseKey + "_Height"] = mediaStream.Height ?? 0;
                        dataObject[baseKey + "_Width"] = mediaStream.Width ?? 0;
                        dataObject[baseKey + "_AspectRatio"] = mediaStream.AspectRatio;
                        dataObject[baseKey + "_Interlaced"] = mediaStream.IsInterlaced;
                        dataObject[baseKey + "_FrameRate"] = mediaStream.RealFrameRate ?? 0;
                        dataObject[baseKey + "_VideoRange"] = mediaStream.VideoRange;
                        dataObject[baseKey + "_ColorSpace"] = mediaStream.ColorSpace;
                        dataObject[baseKey + "_ColorTransfer"] = mediaStream.ColorTransfer;
                        dataObject[baseKey + "_ColorPrimaries"] = mediaStream.ColorPrimaries;
                        dataObject[baseKey + "_PixelFormat"] = mediaStream.PixelFormat;
                        dataObject[baseKey + "_RefFrames"] = mediaStream.RefFrames ?? 0;
                        break;
                    }

                    case MediaStreamType.Subtitle:
                        dataObject[baseKey + "_Title"] = mediaStream.DisplayTitle;
                        dataObject[baseKey + "_Type"] = mediaStream.Type.ToString();
                        dataObject[baseKey + "_Language"] = mediaStream.Language;
                        dataObject[baseKey + "_Codec"] = mediaStream.Codec;
                        dataObject[baseKey + "_Default"] = mediaStream.IsDefault;
                        dataObject[baseKey + "_Forced"] = mediaStream.IsForced;
                        dataObject[baseKey + "_External"] = mediaStream.IsExternal;
                        break;
                }
            }
        }

        return dataObject;
    }

    /// <summary>
    /// Add playback progress data.
    /// </summary>
    /// <param name="dataObject">The data object.</param>
    /// <param name="playbackProgressEventArgs">The playback progress event args.</param>
    /// <returns>The modified data object.</returns>
    public static Dictionary<string, object> AddPlaybackProgressData(this Dictionary<string, object> dataObject, PlaybackProgressEventArgs playbackProgressEventArgs)
    {
        dataObject[nameof(playbackProgressEventArgs.PlaybackPositionTicks)] = playbackProgressEventArgs.PlaybackPositionTicks ?? 0;
        dataObject["PlaybackPosition"] = TimeSpan.FromTicks(playbackProgressEventArgs.PlaybackPositionTicks ?? 0).ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
        dataObject[nameof(playbackProgressEventArgs.MediaSourceId)] = playbackProgressEventArgs.MediaSourceId;
        dataObject[nameof(playbackProgressEventArgs.IsPaused)] = playbackProgressEventArgs.IsPaused;
        dataObject[nameof(playbackProgressEventArgs.IsAutomated)] = playbackProgressEventArgs.IsAutomated;
        dataObject[nameof(playbackProgressEventArgs.DeviceId)] = playbackProgressEventArgs.DeviceId;
        dataObject[nameof(playbackProgressEventArgs.DeviceName)] = playbackProgressEventArgs.DeviceName;
        dataObject[nameof(playbackProgressEventArgs.ClientName)] = playbackProgressEventArgs.ClientName;

        if (playbackProgressEventArgs.Session is not null && playbackProgressEventArgs.Session.PlayState?.PlayMethod is not null)
        {
            dataObject["PlayMethod"] = playbackProgressEventArgs.Session.PlayState.PlayMethod;
        }

        return dataObject;
    }

    /// <summary>
    /// Add user data.
    /// </summary>
    /// <param name="dataObject">The data object.</param>
    /// <param name="user">The user to add.</param>
    /// <returns>The modified data object.</returns>
    public static Dictionary<string, object> AddUserData(this Dictionary<string, object> dataObject, UserDto user)
    {
        dataObject["NotificationUsername"] = user.Name.Escape();
        dataObject["UserId"] = user.Id;
        dataObject[nameof(user.LastLoginDate)] = user.LastLoginDate ?? DateTime.UtcNow;
        dataObject[nameof(user.LastActivityDate)] = user.LastActivityDate ?? DateTime.MinValue;

        return dataObject;
    }

    /// <summary>
    /// Add user data.
    /// </summary>
    /// <param name="dataObject">The data object.</param>
    /// <param name="user">The user to add.</param>
    /// <returns>The modified data object.</returns>
    public static Dictionary<string, object> AddUserData(this Dictionary<string, object> dataObject, User user)
    {
        dataObject["NotificationUsername"] = user.Username.Escape();
        dataObject["UserId"] = user.Id;
        dataObject[nameof(user.LastLoginDate)] = user.LastLoginDate ?? DateTime.UtcNow;
        dataObject[nameof(user.LastActivityDate)] = user.LastActivityDate ?? DateTime.MinValue;

        return dataObject;
    }

    /// <summary>
    /// Add session info data.
    /// </summary>
    /// <param name="dataObject">The data object.</param>
    /// <param name="sessionInfo">The session info to add.</param>
    /// <returns>The modified data object.</returns>
    public static Dictionary<string, object> AddSessionInfoData(this Dictionary<string, object> dataObject, SessionInfo sessionInfo)
    {
        dataObject[nameof(sessionInfo.Id)] = sessionInfo.Id;
        dataObject[nameof(sessionInfo.UserId)] = sessionInfo.UserId;
        dataObject["NotificationUsername"] = sessionInfo.UserName.Escape();
        dataObject[nameof(sessionInfo.Client)] = sessionInfo.Client.Escape();
        dataObject[nameof(sessionInfo.LastActivityDate)] = sessionInfo.LastActivityDate;
        dataObject[nameof(sessionInfo.LastPlaybackCheckIn)] = sessionInfo.LastPlaybackCheckIn;
        dataObject[nameof(sessionInfo.DeviceName)] = sessionInfo.DeviceName.Escape();
        dataObject[nameof(sessionInfo.DeviceId)] = sessionInfo.DeviceId;

        return dataObject;
    }

    /// <summary>
    /// Add plugin installation info.
    /// </summary>
    /// <param name="dataObject">The data object.</param>
    /// <param name="installationInfo">The plugin installation info to add.</param>
    /// <returns>The modified data object.</returns>
    public static Dictionary<string, object> AddPluginInstallationInfo(this Dictionary<string, object> dataObject, InstallationInfo installationInfo)
    {
        dataObject["PluginId"] = installationInfo.Id;
        dataObject["PluginName"] = installationInfo.Name.Escape();
        dataObject["PluginVersion"] = installationInfo.Version;
        dataObject["PluginChangelog"] = installationInfo.Changelog.Escape();
        dataObject["PluginChecksum"] = installationInfo.Checksum;
        dataObject["PluginSourceUrl"] = installationInfo.SourceUrl;

        return dataObject;
    }

    /// <summary>
    /// Add exception info.
    /// </summary>
    /// <param name="dataObject">The data object.</param>
    /// <param name="exception">The exception to add.</param>
    /// <returns>The modified data object.</returns>
    public static Dictionary<string, object> AddExceptionInfo(this Dictionary<string, object> dataObject, Exception exception)
    {
        dataObject["ExceptionMessage"] = exception.Message.Escape();
        dataObject["ExceptionMessageInner"] = exception.InnerException?.Message ?? string.Empty;

        return dataObject;
    }

    /// <summary>
    /// Add user item data.
    /// </summary>
    /// <param name="dataObject">The data object.</param>
    /// <param name="userItemData">The user item data.</param>
    /// <returns>The modified data object.</returns>
    public static Dictionary<string, object> AddUserItemData(this Dictionary<string, object> dataObject, UserItemData userItemData)
    {
        dataObject["Likes"] = userItemData.Likes ?? false;
        dataObject["Rating"] = userItemData.Rating ?? 0;
        dataObject["PlaybackPositionTicks"] = userItemData.PlaybackPositionTicks;
        dataObject["PlaybackPosition"] = TimeSpan.FromTicks(userItemData.PlaybackPositionTicks).ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
        dataObject["PlayCount"] = userItemData.PlayCount;
        dataObject["Favorite"] = userItemData.IsFavorite;
        dataObject["Played"] = userItemData.Played;
        dataObject["AudioStreamIndex"] = userItemData.AudioStreamIndex ?? -1;
        dataObject["SubtitleStreamIndex"] = userItemData.SubtitleStreamIndex ?? -1;
        if (userItemData.LastPlayedDate.HasValue)
        {
            dataObject["LastPlayedDate"] = userItemData.LastPlayedDate;
        }

        return dataObject;
    }

    /// <summary>
    /// Escape quotes for proper json.
    /// </summary>
    /// <param name="input">Input string.</param>
    /// <returns>Escaped string.</returns>
    private static string Escape(this string? input)
        => input?.Replace("\"", "\\\"", StringComparison.Ordinal) ?? string.Empty;
}
