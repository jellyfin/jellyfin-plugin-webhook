using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Configuration;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Destinations.Discord;
using Jellyfin.Plugin.Webhook.Destinations.Generic;
using Jellyfin.Plugin.Webhook.Destinations.Gotify;
using Jellyfin.Plugin.Webhook.Destinations.Pushover;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook
{
    /// <summary>
    /// Webhook sender.
    /// </summary>
    public class WebhookSender
    {
        private readonly ILogger<WebhookSender> _logger;
        private readonly IWebhookClient<DiscordOption> _discordClient;
        private readonly IWebhookClient<GotifyOption> _gotifyClient;
        private readonly IWebhookClient<PushoverOption> _pushoverClient;
        private readonly IWebhookClient<GenericOption> _genericClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebhookSender"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="ILogger{WebhookSender}"/> interface.</param>
        /// <param name="discordClient">Instance of <see cref="IWebhookClient{DiscordOption}"/>.</param>
        /// <param name="gotifyClient">Instance of <see cref="IWebhookClient{GotifyOption}"/>.</param>
        /// <param name="pushoverClient">Instance of the <see cref="IWebhookClient{PushoverClient}"/>.</param>
        /// <param name="genericClient">Instance of the <see cref="IWebhookClient{GenericClient}"/>.</param>
        public WebhookSender(
            ILogger<WebhookSender> logger,
            IWebhookClient<DiscordOption> discordClient,
            IWebhookClient<GotifyOption> gotifyClient,
            IWebhookClient<PushoverOption> pushoverClient,
            IWebhookClient<GenericOption> genericClient)
        {
            _logger = logger;
            _discordClient = discordClient;
            _gotifyClient = gotifyClient;
            _pushoverClient = pushoverClient;
            _genericClient = genericClient;
        }

        private static PluginConfiguration Configuration =>
            WebhookPlugin.Instance?.Configuration
            ?? throw new NullReferenceException(nameof(WebhookPlugin.Instance.Configuration));

        /// <summary>
        /// Send item added notification.
        /// </summary>
        /// <param name="notificationType">The notification type.</param>
        /// <param name="itemData">The item data.</param>
        /// <param name="itemType">The item type.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendItemNotification(NotificationType notificationType, Dictionary<string, object> itemData, Type itemType)
        {
            foreach (var option in Configuration.DiscordOptions.Where(o => o.NotificationTypes.Contains(notificationType)))
            {
                await SendNotification(_discordClient, option, itemData, itemType);
            }

            foreach (var option in Configuration.GotifyOptions.Where(o => o.NotificationTypes.Contains(notificationType)))
            {
                await SendNotification(_gotifyClient, option, itemData, itemType);
            }

            foreach (var option in Configuration.PushoverOptions.Where(o => o.NotificationTypes.Contains(notificationType)))
            {
                await SendNotification(_pushoverClient, option, itemData, itemType);
            }

            foreach (var option in Configuration.GenericOptions.Where(o => o.NotificationTypes.Contains(notificationType)))
            {
                await SendNotification(_genericClient, option, itemData, itemType);
            }
        }

        /// <summary>
        /// Send generic notification.
        /// </summary>
        /// <param name="notificationType">The notification type.</param>
        /// <param name="notificationData">The notification data.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendGenericNotification(NotificationType notificationType, Dictionary<string, object> notificationData)
        {
            foreach (var option in Configuration.DiscordOptions.Where(o => o.NotificationTypes.Contains(notificationType)))
            {
                await SendNotification(_discordClient, option, notificationData);
            }

            foreach (var option in Configuration.GotifyOptions.Where(o => o.NotificationTypes.Contains(notificationType)))
            {
                await SendNotification(_gotifyClient, option, notificationData);
            }

            foreach (var option in Configuration.PushoverOptions.Where(o => o.NotificationTypes.Contains(notificationType)))
            {
                await SendNotification(_pushoverClient, option, notificationData);
            }

            foreach (var option in Configuration.GenericOptions.Where(o => o.NotificationTypes.Contains(notificationType)))
            {
                await SendNotification(_genericClient, option, notificationData);
            }
        }

        private async Task SendNotification<T>(IWebhookClient<T> webhookClient, T option, Dictionary<string, object> itemData, Type itemType)
            where T : BaseOption
        {
            if (NotifyOnItem(option, itemType))
            {
                try
                {
                    await webhookClient.SendAsync(option, itemData);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to send notification.");
                }
            }
        }

        private async Task SendNotification<T>(IWebhookClient<T> webhookClient, T option, Dictionary<string, object> notificationData)
            where T : BaseOption
        {
            try
            {
                await webhookClient.SendAsync(option, notificationData);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to send notification.");
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
    }
}