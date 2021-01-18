using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations.Gotify
{
    /// <inheritdoc />
    public class GotifyClient : IWebhookClient<GotifyOption>
    {
        private readonly ILogger<GotifyClient> _logger;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="GotifyClient"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="ILogger{GotifyDestination}"/> interface.</param>
        /// <param name="httpClient">Instance of the <see cref="HttpClient"/>.</param>
        public GotifyClient(ILogger<GotifyClient> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        /// <inheritdoc />
        public async Task SendAsync(GotifyOption options, Dictionary<string, object> data)
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
                _logger.LogDebug("SendAsync Body: {@Body}", body);
                using var content = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json);
                using var response = await _httpClient.PostAsync(options.WebhookUri.TrimEnd() + $"/message?token={options.Token}", content);
                if (!response.IsSuccessStatusCode)
                {
                    var responseStr = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error sending notification: {Response}", responseStr);
                }
            }
            catch (HttpRequestException e)
            {
                _logger.LogWarning(e, "Error sending notification");
            }
        }
    }
}