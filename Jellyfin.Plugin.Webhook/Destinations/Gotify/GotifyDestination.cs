using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations.Discord;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations.Gotify
{
    /// <summary>
    /// Gotify destination.
    /// </summary>
    public class GotifyDestination : IDestination<GotifyOption>
    {
        private readonly ILogger<GotifyDestination> _logger;
        private readonly IHttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="GotifyDestination"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="ILogger{GotifyDestination}"/> interface.</param>
        /// <param name="httpClient">Instance of the <see cref="IHttpClient"/> interface.</param>
        public GotifyDestination(ILogger<GotifyDestination> logger, IHttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        /// <inheritdoc />
        public Task SendAsync(GotifyOption options, Dictionary<string, object> message)
        {
            throw new System.NotImplementedException();
        }
    }
}