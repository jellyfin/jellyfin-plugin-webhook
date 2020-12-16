using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MediaBrowser.Common.Json;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations.Gotify
{
    /// <inheritdoc />
    public class GotifyClient : IWebhookClient<GotifyOption>
    {
        private readonly ILogger<GotifyClient> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="GotifyClient"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="ILogger{GotifyDestination}"/> interface.</param>
        /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/> interface.</param>
        public GotifyClient(ILogger<GotifyClient> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _jsonSerializerOptions = JsonDefaults.GetOptions();
        }

        /// <inheritdoc />
        public async Task SendItemAddedAsync(GotifyOption options, Dictionary<string, object> data)
        {
            try
            {
                if (string.IsNullOrEmpty(options.WebhookUri))
                {
                    throw new NullReferenceException(nameof(options.WebhookUri));
                }

                // Add gotify specific properties.
                data["Priority"] = options.Priority;

                var body = options.GetCompiledTemplate()(data);
                _logger.LogDebug("SendAsync Body: {@body}", body);
                using var response = await _httpClientFactory
                    .CreateClient(NamedClient.Default)
                    .PostAsJsonAsync(options.WebhookUri.TrimEnd() + $"/message?token={options.Token}", body, _jsonSerializerOptions);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                _logger.LogWarning(e, "Error sending notification.");
            }
        }
    }
}