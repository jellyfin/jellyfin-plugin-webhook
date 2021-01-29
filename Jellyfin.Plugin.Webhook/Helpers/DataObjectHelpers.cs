using System;
using System.Collections.Generic;
using System.Globalization;
using Jellyfin.Data.Entities;
using Jellyfin.Plugin.Webhook.Destinations;
using MediaBrowser.Common;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Updates;

namespace Jellyfin.Plugin.Webhook.Helpers
{
    /// <summary>
    /// Data object helpers.
    /// </summary>
    public static class DataObjectHelpers
    {
        /// <summary>
        /// Gets the default data object.
        /// </summary>
        /// <param name="applicationHost">Instance of the <see cref="IApplicationHost"/> interface.</param>
        /// <param name="notificationType">The notification type.</param>
        /// <returns>The default data object.</returns>
        public static Dictionary<string, object> GetBaseDataObject(IApplicationHost applicationHost, NotificationType notificationType)
        {
            var dataObject = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            dataObject["ServerId"] = applicationHost.SystemId;
            dataObject["ServerName"] = applicationHost.Name.Escape();
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
            if (item == null)
            {
                return dataObject;
            }

            dataObject["Timestamp"] = DateTime.Now;
            dataObject["UtcTimestamp"] = DateTime.UtcNow;
            dataObject["Name"] = item.Name.Escape();
            dataObject["Overview"] = item.Overview.Escape();
            dataObject["ItemId"] = item.Id;
            dataObject["ItemType"] = item.GetType().Name.Escape();
            dataObject["RunTimeTicks"] = item.RunTimeTicks ?? 0;

            if (item.ProductionYear.HasValue)
            {
                dataObject["Year"] = item.ProductionYear;
            }

            switch (item)
            {
                case Season:
                    if (!string.IsNullOrEmpty(item.Parent?.Name))
                    {
                        dataObject["SeriesName"] = item.Parent.Name.Escape();
                    }

                    if (item.Parent?.ProductionYear != null)
                    {
                        dataObject["Year"] = item.Parent.ProductionYear;
                    }

                    if (item.IndexNumber.HasValue)
                    {
                        dataObject["SeasonNumber"] = item.IndexNumber;
                        dataObject["SeasonNumber00"] = item.IndexNumber.Value.ToString("00", CultureInfo.InvariantCulture);
                        dataObject["SeasonNumber000"] = item.IndexNumber.Value.ToString("000", CultureInfo.InvariantCulture);
                    }

                    break;
                case Episode:
                    if (!string.IsNullOrEmpty(item.Parent?.Parent?.Name))
                    {
                        dataObject["SeriesName"] = item.Parent.Parent.Name.Escape();
                    }

                    if (item.Parent?.IndexNumber != null)
                    {
                        dataObject["SeasonNumber"] = item.Parent.IndexNumber;
                        dataObject["SeasonNumber00"] = item.Parent.IndexNumber.Value.ToString("00", CultureInfo.InvariantCulture);
                        dataObject["SeasonNumber000"] = item.Parent.IndexNumber.Value.ToString("000", CultureInfo.InvariantCulture);
                    }

                    if (item.IndexNumber.HasValue)
                    {
                        dataObject["EpisodeNumber"] = item.IndexNumber;
                        dataObject["EpisodeNumber00"] = item.IndexNumber.Value.ToString("00", CultureInfo.InvariantCulture);
                        dataObject["EpisodeNumber000"] = item.IndexNumber.Value.ToString("000", CultureInfo.InvariantCulture);
                    }

                    if (item.Parent?.Parent?.ProductionYear != null)
                    {
                        dataObject["Year"] = item.Parent.Parent.ProductionYear;
                    }

                    break;
            }

            foreach (var (providerKey, providerValue) in item.ProviderIds)
            {
                dataObject[$"Provider_{providerKey.ToLowerInvariant()}"] = providerValue;
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
            dataObject["PlaybackPosition"] = TimeSpan.FromTicks(playbackProgressEventArgs.PlaybackPositionTicks ?? 0).ToString(@"hh\:mm\:ss");
            dataObject[nameof(playbackProgressEventArgs.MediaSourceId)] = playbackProgressEventArgs.MediaSourceId;
            dataObject[nameof(playbackProgressEventArgs.IsPaused)] = playbackProgressEventArgs.IsPaused;
            dataObject[nameof(playbackProgressEventArgs.IsAutomated)] = playbackProgressEventArgs.IsAutomated;
            dataObject[nameof(playbackProgressEventArgs.DeviceId)] = playbackProgressEventArgs.DeviceId;
            dataObject[nameof(playbackProgressEventArgs.DeviceName)] = playbackProgressEventArgs.DeviceName;
            dataObject[nameof(playbackProgressEventArgs.ClientName)] = playbackProgressEventArgs.ClientName;

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
            dataObject["Username"] = user.Name.Escape();
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
            dataObject["Username"] = user.Username.Escape();
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
            dataObject[nameof(sessionInfo.UserName)] = sessionInfo.UserName.Escape();
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
        /// Escape quotes for proper json.
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <returns>Escaped string.</returns>
        private static string Escape(this string? input)
            => input?.Replace("\"", "\\\"") ?? string.Empty;
    }
}