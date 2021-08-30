using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Extensions;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations.Gotify
{
    /// <inheritdoc />
    public class GotifyClient : IWebhookClient<GotifyOption>
    {
        private readonly ILogger<GotifyClient> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="GotifyClient"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="ILogger{GotifyDestination}"/> interface.</param>
        /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/>.</param>
        public GotifyClient(ILogger<GotifyClient> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        /// <inheritdoc />
        public async Task SendAsync(GotifyOption options, Dictionary<string, object> data)
        {
            try
            {
                if (string.IsNullOrEmpty(options.WebhookUri))
                {
                    throw new ArgumentException(nameof(options.WebhookUri));
                }

                if (options.UserFilter.Length != 0
                    && data.TryGetValue("UserId", out var userIdObj)
                    && userIdObj is Guid userId)
                {
                    if (Array.IndexOf(options.UserFilter, userId) == -1)
                    {
                        _logger.LogDebug("UserId {UserId} not found in user filter, ignoring event", userId);
                        return;
                    }
                }

                // Add gotify specific properties.
                data["Priority"] = options.Priority;

                var body = options.GetMessageBody(data);
                _logger.LogDebug("SendAsync Body: {@Body}", body);
                using var content = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json);
                using var response = await _httpClientFactory
                    .CreateClient(NamedClient.Default)
                    .PostAsync(new Uri(options.WebhookUri.TrimEnd() + $"/message?token={options.Token}"), content)
                    .ConfigureAwait(false);
                await response.LogIfFailedAsync(_logger).ConfigureAwait(false);
            }
            catch (HttpRequestException e)
            {
                _logger.LogWarning(e, "Error sending notification");
            }
        }
    }
}
