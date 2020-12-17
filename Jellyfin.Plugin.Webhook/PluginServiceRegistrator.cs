using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Destinations.Discord;
using Jellyfin.Plugin.Webhook.Destinations.Generic;
using Jellyfin.Plugin.Webhook.Destinations.Gotify;
using Jellyfin.Plugin.Webhook.Destinations.Pushover;
using Jellyfin.Plugin.Webhook.Notifiers;
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
            // Register webhook clients.
            serviceCollection.AddSingleton<IWebhookClient<DiscordOption>, DiscordClient>();
            serviceCollection.AddSingleton<IWebhookClient<GotifyOption>, GotifyClient>();
            serviceCollection.AddSingleton<IWebhookClient<PushoverOption>, PushoverClient>();
            serviceCollection.AddSingleton<IWebhookClient<GenericOption>, GenericClient>();

            // Register sender.
            serviceCollection.AddSingleton<WebhookSender>();

            // Register event consumers.
            serviceCollection.AddScoped<IEventConsumer<PlaybackStartEventArgs>, PlaybackStartNotifier>();
        }
    }
}