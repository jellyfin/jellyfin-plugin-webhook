using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Configuration;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Destinations.Discord;
using Jellyfin.Plugin.Webhook.Destinations.Generic;
using Jellyfin.Plugin.Webhook.Destinations.GenericForm;
using Jellyfin.Plugin.Webhook.Destinations.Gotify;
using Jellyfin.Plugin.Webhook.Destinations.Mqtt;
using Jellyfin.Plugin.Webhook.Destinations.Pushbullet;
using Jellyfin.Plugin.Webhook.Destinations.Pushover;
using Jellyfin.Plugin.Webhook.Destinations.Slack;
using Jellyfin.Plugin.Webhook.Destinations.Smtp;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook;

/// <inheritdoc />
public class WebhookSender : IWebhookSender
{
    private readonly ILogger<WebhookSender> _logger;
    private readonly IWebhookClient<DiscordOption> _discordClient;
    private readonly IWebhookClient<GenericOption> _genericClient;
    private readonly IWebhookClient<GenericFormOption> _genericFormClient;
    private readonly IWebhookClient<GotifyOption> _gotifyClient;
    private readonly IWebhookClient<PushbulletOption> _pushbulletClient;
    private readonly IWebhookClient<PushoverOption> _pushoverClient;
    private readonly IWebhookClient<SlackOption> _slackClient;
    private readonly IWebhookClient<SmtpOption> _smtpClient;
    private readonly IWebhookClient<MqttOption> _mqttClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookSender"/> class.
    /// </summary>
    /// <param name="logger">Instance of the <see cref="ILogger{WebhookSender}"/> interface.</param>
    /// <param name="discordClient">Instance of <see cref="IWebhookClient{DiscordOption}"/>.</param>
    /// /// <param name="genericClient">Instance of the <see cref="IWebhookClient{GenericOption}"/>.</param>
    /// <param name="genericFormClient">Instance of the <see cref="IWebhookClient{GenericFormOption}"/>.</param>
    /// <param name="gotifyClient">Instance of <see cref="IWebhookClient{GotifyOption}"/>.</param>
    /// <param name="pushbulletClient">Instance of the <see cref="IWebhookClient{PushbulletOption}"/>.</param>
    /// <param name="pushoverClient">Instance of the <see cref="IWebhookClient{PushoverOption}"/>.</param>
    /// <param name="slackClient">Instance of the <see cref="IWebhookClient{SlackOption}"/>.</param>
    /// <param name="smtpClient">Instance of the <see cref="IWebhookClient{SmtpOption}"/>.</param>
    /// <param name="mqttClient">Instance of the <see cref="IWebhookClient{mqttClient}"/>.</param>
    public WebhookSender(
        ILogger<WebhookSender> logger,
        IWebhookClient<DiscordOption> discordClient,
        IWebhookClient<GenericOption> genericClient,
        IWebhookClient<GenericFormOption> genericFormClient,
        IWebhookClient<GotifyOption> gotifyClient,
        IWebhookClient<PushbulletOption> pushbulletClient,
        IWebhookClient<PushoverOption> pushoverClient,
        IWebhookClient<SlackOption> slackClient,
        IWebhookClient<SmtpOption> smtpClient,
        IWebhookClient<MqttOption> mqttClient)
    {
        _logger = logger;
        _discordClient = discordClient;
        _genericClient = genericClient;
        _genericFormClient = genericFormClient;
        _gotifyClient = gotifyClient;
        _pushbulletClient = pushbulletClient;
        _pushoverClient = pushoverClient;
        _slackClient = slackClient;
        _smtpClient = smtpClient;
        _mqttClient = mqttClient;
    }

    private static PluginConfiguration Configuration =>
        WebhookPlugin.Instance!.Configuration;

    /// <inheritdoc />
    public async Task SendNotification(NotificationType notificationType, Dictionary<string, object> itemData, Type? itemType = null)
    {
        foreach (var option in Configuration.DiscordOptions.Where(o => o.NotificationTypes.Contains(notificationType)))
        {
            await SendNotification(_discordClient, option, itemData, itemType)
                .ConfigureAwait(false);
        }

        foreach (var option in Configuration.GenericOptions.Where(o => o.NotificationTypes.Contains(notificationType)))
        {
            await SendNotification(_genericClient, option, itemData, itemType)
                .ConfigureAwait(false);
        }

        foreach (var option in Configuration.GenericFormOptions.Where(o => o.NotificationTypes.Contains(notificationType)))
        {
            await SendNotification(_genericFormClient, option, itemData, itemType)
                .ConfigureAwait(false);
        }

        foreach (var option in Configuration.GotifyOptions.Where(o => o.NotificationTypes.Contains(notificationType)))
        {
            await SendNotification(_gotifyClient, option, itemData, itemType)
                .ConfigureAwait(false);
        }

        foreach (var option in Configuration.PushbulletOptions.Where(o => o.NotificationTypes.Contains(notificationType)))
        {
            await SendNotification(_pushbulletClient, option, itemData, itemType)
                .ConfigureAwait(false);
        }

        foreach (var option in Configuration.PushoverOptions.Where(o => o.NotificationTypes.Contains(notificationType)))
        {
            await SendNotification(_pushoverClient, option, itemData, itemType)
                .ConfigureAwait(false);
        }

        foreach (var option in Configuration.SlackOptions.Where(o => o.NotificationTypes.Contains(notificationType)))
        {
            await SendNotification(_slackClient, option, itemData, itemType)
                .ConfigureAwait(false);
        }

        foreach (var option in Configuration.SmtpOptions.Where(o => o.NotificationTypes.Contains(notificationType)))
        {
            await SendNotification(_smtpClient, option, itemData, itemType)
                .ConfigureAwait(false);
        }

        foreach (var option in Configuration.MqttOptions.Where(o => o.NotificationTypes.Contains(notificationType)))
        {
            await SendNotification(_mqttClient, option, itemData, itemType)
                .ConfigureAwait(false);
        }
    }

    private static bool NotifyOnItem<T>(T baseOptions, Type? itemType)
        where T : BaseOption
    {
        if (itemType is null)
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
                await webhookClient.SendAsync(option, itemData)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to send notification");
            }
        }
    }
}
