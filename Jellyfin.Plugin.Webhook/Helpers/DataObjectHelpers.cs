using System;
using System.Collections.Generic;
using System.Globalization;
using Jellyfin.Plugin.Webhook.Destinations;
using MediaBrowser.Common;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Dto;

namespace Jellyfin.Plugin.Webhook.Helpers
{
    /// <summary>
    /// Data object helpers.
    /// </summary>
    public class DataObjectHelpers
    {
        /// <summary>
        /// Get data object from <see cref="BaseItem"/>.
        /// </summary>
        /// <param name="applicationHost">Instance of the <see cref="IApplicationHost"/> interface.</param>
        /// <param name="item">Instance of the <see cref="BaseItem"/>.</param>
        /// <returns>The data object.</returns>
        public static Dictionary<string, object> GetBaseItemDataObject(IApplicationHost applicationHost, BaseItem item)
        {
            var data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["Timestamp"] = DateTime.Now,
                ["UtcTimestamp"] = DateTime.UtcNow,
                ["Name"] = item.Name,
                ["Overview"] = item.Overview,
                ["ItemId"] = item.Id,
                ["ServerId"] = applicationHost.SystemId,
                ["ServerUrl"] = WebhookPlugin.Instance?.Configuration.ServerUrl ?? "localhost:8096",
                ["ServerName"] = applicationHost.Name,
                ["ItemType"] = item.GetType().Name
            };

            if (item.ProductionYear.HasValue)
            {
                data["Year"] = item.ProductionYear;
            }

            switch (item)
            {
                case Season:
                    if (!string.IsNullOrEmpty(item.Parent?.Name))
                    {
                        data["SeriesName"] = item.Parent.Name;
                    }

                    if (item.Parent?.ProductionYear != null)
                    {
                        data["Year"] = item.Parent.ProductionYear;
                    }

                    if (item.IndexNumber.HasValue)
                    {
                        data["SeasonNumber"] = item.IndexNumber;
                        data["SeasonNumber00"] = item.IndexNumber.Value.ToString("00", CultureInfo.InvariantCulture);
                        data["SeasonNumber000"] = item.IndexNumber.Value.ToString("000", CultureInfo.InvariantCulture);
                    }

                    break;
                case Episode:
                    if (!string.IsNullOrEmpty(item.Parent?.Parent?.Name))
                    {
                        data["SeriesName"] = item.Parent.Parent.Name;
                    }

                    if (item.Parent?.IndexNumber != null)
                    {
                        data["SeasonNumber"] = item.Parent.IndexNumber;
                        data["SeasonNumber00"] = item.Parent.IndexNumber.Value.ToString("00", CultureInfo.InvariantCulture);
                        data["SeasonNumber000"] = item.Parent.IndexNumber.Value.ToString("000", CultureInfo.InvariantCulture);
                    }

                    if (item.IndexNumber.HasValue)
                    {
                        data["EpisodeNumber"] = item.IndexNumber;
                        data["EpisodeNumber00"] = item.IndexNumber.Value.ToString("00", CultureInfo.InvariantCulture);
                        data["EpisodeNumber000"] = item.IndexNumber.Value.ToString("000", CultureInfo.InvariantCulture);
                    }

                    if (item.Parent?.Parent?.ProductionYear != null)
                    {
                        data["Year"] = item.Parent.Parent.ProductionYear;
                    }

                    break;
            }

            foreach (var (providerKey, providerValue) in item.ProviderIds)
            {
                data[$"Provider_{providerKey.ToLowerInvariant()}"] = providerValue;
            }

            return data;
        }

        /// <summary>
        /// Get data object from <see cref="BaseItemDto"/>.
        /// </summary>
        /// <param name="applicationHost">Instance of the <see cref="IApplicationHost"/> interface.</param>
        /// <param name="item">Instance of the <see cref="BaseItemDto"/>.</param>
        /// <returns>The data object.</returns>
        public static Dictionary<string, object> GetBaseItemDtoDataObject(IApplicationHost applicationHost, BaseItemDto item)
        {
            var data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["Timestamp"] = DateTime.Now,
                ["UtcTimestamp"] = DateTime.UtcNow,
                ["Name"] = item.Name,
                ["Overview"] = item.Overview,
                ["ItemId"] = item.Id,
                ["ServerId"] = applicationHost.SystemId,
                ["ServerUrl"] = WebhookPlugin.Instance?.Configuration.ServerUrl ?? "localhost:8096",
                ["ServerName"] = applicationHost.Name,
                ["ItemType"] = item.GetType().Name
            };

            if (item.ProductionYear.HasValue)
            {
                data["Year"] = item.ProductionYear;
            }

            if (item.IndexNumber.HasValue)
            {
                data["EpisodeNumber"] = item.IndexNumber;
                data["EpisodeNumber00"] = item.IndexNumber.Value.ToString("00", CultureInfo.InvariantCulture);
                data["EpisodeNumber000"] = item.IndexNumber.Value.ToString("000", CultureInfo.InvariantCulture);
            }

            foreach (var (providerKey, providerValue) in item.ProviderIds)
            {
                data[$"Provider_{providerKey.ToLowerInvariant()}"] = providerValue;
            }

            return data;
        }
    }
}