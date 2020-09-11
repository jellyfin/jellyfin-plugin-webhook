using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Net;
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
        public async Task SendAsync(GotifyOption options, Dictionary<string, object> data)
        {
            try
            {
                // Add gotify specific properties.
                data["Priority"] = options.Priority;

                var body = options.GetCompiledTemplate()(data);
                _logger.LogTrace("SendAsync Body: {body}", body);
                var requestOptions = new HttpRequestOptions
                {
                    Url = options.WebhookUri.TrimEnd() + $"/message?token={options.Token}",
                    RequestContent = body,
                    RequestContentType = MediaTypeNames.Application.Json
                };

                using var response = await _httpClient.Post(requestOptions).ConfigureAwait(false);
            }
            catch (HttpException e)
            {
                _logger.LogWarning(e, "Error sending notification.");
            }
        }
    }
}