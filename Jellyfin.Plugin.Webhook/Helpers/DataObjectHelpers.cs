using System;
using System.Collections.Generic;
using System.Globalization;
using MediaBrowser.Common;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;

namespace Jellyfin.Plugin.Webhook.Helpers
{
    /// <summary>
    /// Data object helpers.
    /// </summary>
    public static class DataObjectHelpers
    {
        /// <summary>
        /// Get data object from <see cref="BaseItem"/>.
        /// </summary>
        /// <param name="dataObject">The existing data object.</param>
        /// <param name="applicationHost">Instance of the <see cref="IApplicationHost"/> interface.</param>
        /// <param name="item">Instance of the <see cref="BaseItem"/>.</param>
        /// <returns>The data object.</returns>
        public static Dictionary<string, object> AddBaseItemData(this Dictionary<string, object> dataObject, IApplicationHost applicationHost, BaseItem item)
        {
            dataObject["Timestamp"] = DateTime.Now;
            dataObject["UtcTimestamp"] = DateTime.UtcNow;
            dataObject["Name"] = item.Name;
            dataObject["Overview"] = item.Overview;
            dataObject["ItemId"] = item.Id;
            dataObject["ServerId"] = applicationHost.SystemId;
            dataObject["ServerUrl"] = WebhookPlugin.Instance?.Configuration.ServerUrl ?? "localhost:8096";
            dataObject["ServerName"] = applicationHost.Name;
            dataObject["ItemType"] = item.GetType().Name;

            if (item.ProductionYear.HasValue)
            {
                dataObject["Year"] = item.ProductionYear;
            }

            switch (item)
            {
                case Season:
                    if (!string.IsNullOrEmpty(item.Parent?.Name))
                    {
                        dataObject["SeriesName"] = item.Parent.Name;
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
                        dataObject["SeriesName"] = item.Parent.Parent.Name;
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
            dataObject[nameof(playbackProgressEventArgs.MediaSourceId)] = playbackProgressEventArgs.MediaSourceId;
            dataObject[nameof(playbackProgressEventArgs.IsPaused)] = playbackProgressEventArgs.IsPaused;
            dataObject[nameof(playbackProgressEventArgs.IsAutomated)] = playbackProgressEventArgs.IsAutomated;
            dataObject[nameof(playbackProgressEventArgs.DeviceId)] = playbackProgressEventArgs.DeviceId;
            dataObject[nameof(playbackProgressEventArgs.DeviceName)] = playbackProgressEventArgs.DeviceName;
            dataObject[nameof(playbackProgressEventArgs.ClientName)] = playbackProgressEventArgs.ClientName;

            return dataObject;
        }
    }
}