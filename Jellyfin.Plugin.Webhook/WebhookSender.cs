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
using Jellyfin.Plugin.Webhook.Destinations.Smtp;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook
{
    /// <inheritdoc />
    public class WebhookSender : IWebhookSender
    {
        private readonly ILogger<WebhookSender> _logger;
        private readonly IWebhookClient<DiscordOption> _discordClient;
        private readonly IWebhookClient<GotifyOption> _gotifyClient;
        private readonly IWebhookClient<PushoverOption> _pushoverClient;
        private readonly IWebhookClient<GenericOption> _genericClient;
        private readonly IWebhookClient<SmtpOption> _smtpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebhookSender"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="ILogger{WebhookSender}"/> interface.</param>
        /// <param name="discordClient">Instance of <see cref="IWebhookClient{DiscordOption}"/>.</param>
        /// <param name="gotifyClient">Instance of <see cref="IWebhookClient{GotifyOption}"/>.</param>
        /// <param name="pushoverClient">Instance of the <see cref="IWebhookClient{PushoverOption}"/>.</param>
        /// <param name="genericClient">Instance of the <see cref="IWebhookClient{GenericOption}"/>.</param>
        /// <param name="smtpClient">Instance of the <see cref="IWebhookClient{SmtpOption}"/>.</param>
        public WebhookSender(
            ILogger<WebhookSender> logger,
            IWebhookClient<DiscordOption> discordClient,
            IWebhookClient<GotifyOption> gotifyClient,
            IWebhookClient<PushoverOption> pushoverClient,
            IWebhookClient<GenericOption> genericClient,
            IWebhookClient<SmtpOption> smtpClient)
        {
            _logger = logger;
            _discordClient = discordClient;
            _gotifyClient = gotifyClient;
            _pushoverClient = pushoverClient;
            _genericClient = genericClient;
            _smtpClient = smtpClient;
        }

        private static PluginConfiguration Configuration =>
            WebhookPlugin.Instance?.Configuration
            ?? throw new NullReferenceException(nameof(WebhookPlugin.Instance.Configuration));

        /// <inheritdoc />
        public async Task SendNotification(NotificationType notificationType, Dictionary<string, object> itemData, Type? itemType = null)
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

            foreach (var option in Configuration.SmtpOptions.Where(o => o.NotificationTypes.Contains(notificationType)))
            {
                await SendNotification(_smtpClient, option, itemData, itemType);
            }
        }

        private static bool NotifyOnItem<T>(T baseOptions, Type? itemType)
            where T : BaseOption
        {
            if (itemType == null)
            {
                return true;
            }

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

        private async Task SendNotification<T>(IWebhookClient<T> webhookClient, T option, Dictionary<string, object> itemData, Type? itemType)
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
                    _logger.LogError(e, "Unable to send notification");
                }
            }
        }
    }
}