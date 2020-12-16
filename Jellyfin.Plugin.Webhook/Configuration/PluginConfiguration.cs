using System;
using System.Collections.Generic;
using Jellyfin.Plugin.Webhook.Destinations.Discord;
using Jellyfin.Plugin.Webhook.Destinations.Generic;
using Jellyfin.Plugin.Webhook.Destinations.Gotify;
using Jellyfin.Plugin.Webhook.Destinations.Pushover;
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
            ServerUrl = string.Empty;
            DiscordOptions = Array.Empty<DiscordOption>();
            GotifyOptions = Array.Empty<GotifyOption>();
            PushoverOptions = Array.Empty<PushoverOption>();
            GenericOptions = Array.Empty<GenericOption>();
        }

        /// <summary>
        /// Gets or sets the jellyfin server url.
        /// </summary>
        public string ServerUrl { get; set; }

        /// <summary>
        /// Gets or sets the discord options.
        /// </summary>
        public IReadOnlyList<DiscordOption> DiscordOptions { get; set; }

        /// <summary>
        /// Gets or sets the gotify options.
        /// </summary>
        public IReadOnlyList<GotifyOption> GotifyOptions { get; set; }

        /// <summary>
        /// Gets or sets the pushover options.
        /// </summary>
        public IReadOnlyList<PushoverOption> PushoverOptions { get; set; }

        /// <summary>
        /// Gets or sets the generic options.
        /// </summary>
        public IReadOnlyList<GenericOption> GenericOptions { get; set; }
    }
}