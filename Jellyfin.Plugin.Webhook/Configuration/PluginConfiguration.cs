using System;
using Jellyfin.Plugin.Webhook.Destinations.Discord;
using Jellyfin.Plugin.Webhook.Destinations.Gotify;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Webhook.Configuration
{
    /// <summary>
    /// Webhook plugin configuration.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
        /// </summary>
        public PluginConfiguration()
        {
            DiscordOptions = Array.Empty<DiscordOption>();
            GotifyOptions = Array.Empty<GotifyOption>();
        }

        /// <summary>
        /// Gets or sets the jellyfin server url.
        /// </summary>
        public string ServerUrl { get; set; }

        /// <summary>
        /// Gets or sets the discord options.
        /// </summary>
        public DiscordOption[] DiscordOptions { get; set; }

        /// <summary>
        /// Gets or sets the gotify options.
        /// </summary>
        public GotifyOption[] GotifyOptions { get; set; }
    }
}