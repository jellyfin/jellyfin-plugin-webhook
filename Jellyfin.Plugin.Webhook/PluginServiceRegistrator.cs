using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Destinations.Discord;
using Jellyfin.Plugin.Webhook.Destinations.Generic;
using Jellyfin.Plugin.Webhook.Destinations.Gotify;
using Jellyfin.Plugin.Webhook.Destinations.Pushover;
using Jellyfin.Plugin.Webhook.Notifiers;
using MediaBrowser.Common.Net;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.Webhook
{
    /// <summary>
    /// Register webhook services.
    /// </summary>
    public class PluginServiceRegistrator : IPluginServiceRegistrator
    {
        /// <inheritdoc />
        public void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHttpClient<IWebhookClient<DiscordOption>, DiscordClient>()
                .ConfigurePrimaryHttpMessageHandler(_ => new DefaultHttpClientHandler());
            serviceCollection.AddHttpClient<IWebhookClient<GotifyOption>, GotifyClient>()
                .ConfigurePrimaryHttpMessageHandler(_ => new DefaultHttpClientHandler());
            serviceCollection.AddHttpClient<IWebhookClient<PushoverOption>, PushoverClient>()
                .ConfigurePrimaryHttpMessageHandler(_ => new DefaultHttpClientHandler());
            serviceCollection.AddHttpClient<IWebhookClient<GenericOption>, GenericClient>()
                .ConfigurePrimaryHttpMessageHandler(_ => new DefaultHttpClientHandler());

            // Register sender.
            serviceCollection.AddScoped<WebhookSender>();

            // Register event consumers.
            serviceCollection.AddScoped<IEventConsumer<PlaybackStartEventArgs>, PlaybackStartNotifier>();
            serviceCollection.AddScoped<IEventConsumer<PlaybackProgressEventArgs>, PlaybackProgressNotifier>();
            serviceCollection.AddScoped<IEventConsumer<PlaybackStopEventArgs>, PlaybackStopNotifier>();
        }
    }
}