using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Entities;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Destinations.Discord;
using Jellyfin.Plugin.Webhook.Destinations.Gotify;
using Jellyfin.Plugin.Webhook.Helpers;
using Jellyfin.Plugin.Webhook.Models;
using MediaBrowser.Common;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Notifications;
using Microsoft.Extensions.Logging;
using Constants = Jellyfin.Plugin.Webhook.Configuration.Constants;
using Episode = MediaBrowser.Controller.Entities.TV.Episode;
using Movie = MediaBrowser.Controller.Entities.Movies.Movie;
using MusicAlbum = MediaBrowser.Controller.Entities.Audio.MusicAlbum;
using Season = MediaBrowser.Controller.Entities.TV.Season;
using Series = MediaBrowser.Controller.Entities.TV.Series;

namespace Jellyfin.Plugin.Webhook.Notifiers
{
    /// <summary>
    /// Notifier when a library item is added.
    /// </summary>
    public class LibraryAddedNotifier : INotificationService, IDisposable
    {
        private readonly ILogger<LibraryAddedNotifier> _logger;
        private readonly ILibraryManager _libraryManager;
        private readonly IApplicationHost _applicationHost;

        private readonly ConcurrentDictionary<Guid, QueuedItemContainer> _itemProcessQueue;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly DiscordDestination _discordDestination;
        private readonly GotifyDestination _gotifyDestination;

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryAddedNotifier"/> class.
        /// </summary>
        /// <param name="loggerFactory">Instance of the <see cref="ILoggerFactory"/> interface.</param>
        /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
        /// <param name="httpClient">Instance of the <see cref="IHttpClient"/> interface.</param>
        /// <param name="applicationHost">Instance of the <see cref="IApplicationHost"/> interface.</param>
        public LibraryAddedNotifier(ILoggerFactory loggerFactory, ILibraryManager libraryManager, IHttpClient httpClient, IApplicationHost applicationHost)
        {
            _logger = loggerFactory.CreateLogger<LibraryAddedNotifier>();
            _libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
            _applicationHost = applicationHost;

            _itemProcessQueue = new ConcurrentDictionary<Guid, QueuedItemContainer>();
            _libraryManager.ItemAdded += ItemAddedHandler;

            HandlebarsFunctionHelpers.RegisterHelpers();
            _cancellationTokenSource = new CancellationTokenSource();
            PeriodicAsyncHelper.PeriodicAsync(
                    async () =>
                    {
                        try
                        {
                            await ProcessItemsAsync().ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error");
                        }
                    }, TimeSpan.FromMilliseconds(Constants.RecheckIntervalMs),
                    _cancellationTokenSource.Token)
                .ConfigureAwait(false);

            _discordDestination = new DiscordDestination(
                loggerFactory.CreateLogger<DiscordDestination>(),
                httpClient);

            _gotifyDestination = new GotifyDestination(
                loggerFactory.CreateLogger<GotifyDestination>(),
                httpClient);

            /*var c = new PluginConfiguration
            {
                ServerUrl = "http://localhost:8096",
                DiscordOptions = new[]
                {
                    new DiscordOptions
                    {
                        AvatarUrl = "https://raw.githubusercontent.com/jellyfin/jellyfin-ux/master/branding/NSIS/modern-install.png",
                        EmbedColor = "#aa5cc3",
                        EnableAlbums = true,
                        EnableSeasons = true,
                        EnableEpisodes = true,
                        EnableMovies = true,
                        EnableSeries = true,
                        EnableSongs = true,
                        Username = "jellybot",
                        MentionType = DiscordMentionType.Here,
                        Template = "ewogICJjb250ZW50IjogInt7TWVudGlvblR5cGV9fSIsCiAgImF2YXRhcl91cmwiOiAie3tBdmF0YXJVcmx9fSIsCiAgInVzZXJuYW1lIjogInt7VXNlcm5hbWV9fSIKICAiZW1iZWRzIjogWwoJewoJCSJjb2xvciI6ICJ7e0VtYmVkQ29sb3J9fSIsCgkJImZvb3RlciI6IHsKCQkJInRleHQiOiAiRnJvbSB7e1NlcnZlck5hbWV9fSIsCgkJCSJpY29uVXJsIjogInt7QXZhdGFyVXJsfX0iCgkJfSwKCQkidGltZXN0YW1wIjogInt7VXRjVGltZXN0YW1wfX0iLAogICAgICAgIHt7I2lzX2VxdWFscyBJdGVtVHlwZSAnU2Vhc29uJ319CiAgICAgICAgICAgICJ0aXRsZSI6ICJ7e1Nlcmllc05hbWV9fSB7e05hbWV9fSBoYXMgYmVlbiBhZGRlZCB0byB7e1NlcnZlck5hbWV9fSIKICAgICAgICB7e2Vsc2V9fQogICAgICAgIHt7I2lzX2VxdWFscyBJdGVtVHlwZSAnRXBpc29kZSd9fQogICAgICAgICAgICAidGl0bGUiOiAie3tTZXJpZXNOYW1lfX0gU3t7U2Vhc29uTnVtYmVyfX1Fe3tFcGlzb2RlTnVtYmVyfX0ge3tOYW1lfX0gaGFzIGJlZW4gYWRkZWQgdG8ge3tTZXJ2ZXJOYW1lfX0iCiAgICAgICAge3tlbHNlfX0KICAgICAgICAgICAgInRpdGxlIjogInt7TmFtZX19ICh7e1llYXJ9fSkgaGFzIGJlZW4gYWRkZWQgdG8ge3tTZXJ2ZXJOYW1lfX0iICAgICAgICAKICAgICAgICB7ey9pc19lcXVhbHN9fSAgICAgICAKICAgICAgICB7ey9pc19lcXVhbHN9fQogICAgICAgICJ1cmwiOiAie3tTZXJ2ZXJVcmx9fS93ZWIvaW5kZXguaHRtbC8jIS9kZXRhaWxzP2lkPXt7SXRlbUlkfX0mc2VydmVySWQ9e3tTZXJ2ZXJJZH19IiwKICAgICAgICAidGh1bWJuYWlsIjp7CiAgICAgICAgICAgICJ1cmwiOiAie3tTZXJ2ZXJVcmx9fS9JdGVtcy97e0l0ZW1JZH19L0ltYWdlcy9QcmltYXJ5IgogICAgICAgIH0KCX0KICBdCn0=",
                        WebhookUri = "https://discordapp.com/api/webhooks/752725480880210000/xJSIE0Pp8E73ojyqXjwPk5EKD3syDabQWBQbmFvqlN9YVnVWPOxcr24oCrJ43Zd415gq"
                    },
                }
            };

            WebhookPlugin.Instance.Configuration.ServerUrl = c.ServerUrl;
            WebhookPlugin.Instance.Configuration.DiscordOptions = c.DiscordOptions;
            WebhookPlugin.Instance.SaveConfiguration();*/
        }

        /// <inheritdoc />
        public string Name => WebhookPlugin.Instance.Name;

        /// <inheritdoc />
        public Task SendNotification(UserNotification request, CancellationToken cancellationToken) => Task.CompletedTask;

        /// <inheritdoc />
        public bool IsEnabledForUser(User user) => true;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Dispose all assets.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _libraryManager.ItemAdded -= ItemAddedHandler;
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }
        }

        private bool NotifyOnItem<T>(T baseOptions, Type itemType)
            where T : BaseOption
        {
            _logger.LogDebug("NotifyOnItem");
            if (baseOptions.EnableAlbums && itemType == typeof(MusicAlbum))
            {
                return true;
            }

            if (baseOptions.EnableMovies && itemType == typeof(Movie))
            {
                return true;
            }

            if (baseOptions.EnableEpisodes && itemType == typeof(Episode))
            {
                return true;
            }

            if (baseOptions.EnableSeries && itemType == typeof(Series))
            {
                return true;
            }

            if (baseOptions.EnableSeasons && itemType == typeof(Season))
            {
                return true;
            }

            if (baseOptions.EnableSongs && itemType == typeof(Audio))
            {
                return true;
            }

            return false;
        }

        private void ItemAddedHandler(object sender, ItemChangeEventArgs itemChangeEventArgs)
        {
            // Never notify on virtual items.
            if (itemChangeEventArgs.Item.IsVirtualItem)
            {
                return;
            }

            _itemProcessQueue.TryAdd(itemChangeEventArgs.Item.Id, new QueuedItemContainer(itemChangeEventArgs.Item.Id));
            _logger.LogDebug("Queued {itemName} for notification.", itemChangeEventArgs.Item.Name);
        }

        private async Task ProcessItemsAsync()
        {
            _logger.LogDebug("ProcessItemsAsync");
            // Attempt to process all items in queue.
            var currentItems = _itemProcessQueue.ToArray();
            foreach (var (key, container) in currentItems)
            {
                var item = _libraryManager.GetItemById(key);
                _logger.LogDebug("Item {itemName}", item.Name);

                // Metadata not refreshed yet and under retry limit.
                if (item.ProviderIds.Keys.Count == 0 && container.RetryCount < Constants.MaxRetries)
                {
                    _logger.LogDebug("Requeue {itemName}, no provider ids.", item.Name);
                    container.RetryCount++;
                    _itemProcessQueue.AddOrUpdate(key, container, (_, __) => container);
                    continue;
                }

                _logger.LogDebug("Notifying for {itemName}", item.Name);

                // Send notification to each configured destination.
                var itemData = GetDataObject(item);
                var itemType = item.GetType();
                foreach (var option in WebhookPlugin.Instance.Configuration.DiscordOptions)
                {
                    await SendNotification(_discordDestination, option, itemData, itemType).ConfigureAwait(false);
                }

                foreach (var option in WebhookPlugin.Instance.Configuration.GotifyOptions)
                {
                    await SendNotification(_gotifyDestination, option, itemData, itemType).ConfigureAwait(false);
                }

                // Remove item from queue.
                _itemProcessQueue.TryRemove(key, out _);
            }
        }

        private async Task SendNotification<T>(IDestination<T> destination, T option, Dictionary<string, object> itemData, Type itemType)
            where T : BaseOption
        {
            if (NotifyOnItem(option, itemType))
            {
                try
                {
                    await destination.SendAsync(
                            option,
                            itemData)
                        .ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to send webhook.");
                }
            }
        }

        private Dictionary<string, object> GetDataObject(BaseItem item)
        {
            var data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            data["Timestamp"] = DateTime.Now;
            data["UtcTimestamp"] = DateTime.UtcNow;
            data["Name"] = item.Name;
            data["ItemId"] = item.Id;
            data["ServerId"] = _applicationHost.SystemId;
            data["ServerUrl"] = WebhookPlugin.Instance.Configuration.ServerUrl;
            data["ServerName"] = _applicationHost.Name;
            data["ItemType"] = item.GetType().Name;

            if (!item.ProductionYear.HasValue)
            {
                data["Year"] = item.ProductionYear;
            }

            switch (item)
            {
                case Season _:
                    if (!string.IsNullOrEmpty(item.Parent?.Name))
                    {
                        data["SeriesName"] = item.Parent.Name;
                    }

                    if (item.Parent?.ProductionYear.HasValue ?? false)
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
                case Episode _:
                    if (!string.IsNullOrEmpty(item.Parent?.Parent?.Name))
                    {
                        data["SeriesName"] = item.Parent.Parent.Name;
                    }

                    if (item.Parent?.IndexNumber.HasValue ?? false)
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

                    if (item.Parent?.Parent?.ProductionYear.HasValue ?? false)
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
    }
}