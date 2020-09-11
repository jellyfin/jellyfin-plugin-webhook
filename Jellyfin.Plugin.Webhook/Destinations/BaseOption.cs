using System;
using HandlebarsDotNet;
using Jellyfin.Plugin.Webhook.Helpers;

namespace Jellyfin.Plugin.Webhook.Destinations
{
    /// <summary>
    /// Base options for destination.
    /// </summary>
    public abstract class BaseOption
    {
        private Func<object, string> _compiledTemplate;

        /// <summary>
        /// Gets or sets the webhook uri.
        /// </summary>
        public string WebhookUri { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to notify on movies.
        /// </summary>
        public bool EnableMovies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to notify on episodes.
        /// </summary>
        public bool EnableEpisodes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to notify on series.
        /// </summary>
        public bool EnableSeries { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to notify on seasons.
        /// </summary>
        public bool EnableSeasons { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to notify on albums.
        /// </summary>
        public bool EnableAlbums { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to notify on songs.
        /// </summary>
        public bool EnableSongs { get; set; }

        /// <summary>
        /// Gets or sets the handlebars template.
        /// </summary>
        public string Template { get; set; }

        /// <summary>
        /// Gets the compiled handlebars template.
        /// </summary>
        /// <returns>The compiled handlebars template.</returns>
        public Func<object, string> GetCompiledTemplate()
        {
            return _compiledTemplate ??= Handlebars.Compile(HandlebarsFunctionHelpers.Base64Decode(Template));
        }
    }
}