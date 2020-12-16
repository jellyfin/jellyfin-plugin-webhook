using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MediaBrowser.Common.Json;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations.Pushover
{
    /// <inheritdoc />
    public class PushoverClient : IWebhookClient<PushoverOption>
    {
        private readonly ILogger<PushoverClient> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="PushoverClient"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="ILogger{PushoverClient}"/> interface.</param>
        /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/> interface.</param>
        public PushoverClient(ILogger<PushoverClient> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _jsonSerializerOptions = JsonDefaults.GetOptions();
        }

        /// <inheritdoc />
        public async Task SendItemAddedAsync(PushoverOption options, Dictionary<string, object> data)
        {
            try
            {
                data["Token"] = options.Token;
                data["UserToken"] = options.UserToken;
                if (!string.IsNullOrEmpty(options.Device))
                {
                    data["Device"] = options.Device;
                }

                if (!string.IsNullOrEmpty(options.Title))
                {
                    data["Title"] = options.Title;
                }

                if (!string.IsNullOrEmpty(options.MessageUrl))
                {
                    data["MessageUrl"] = options.MessageUrl;
                }

                if (!string.IsNullOrEmpty(options.MessageUrlTitle))
                {
                    data["MessageUrlTitle"] = options.MessageUrlTitle;
                }

                if (!string.IsNullOrEmpty(options.MessagePriority))
                {
                    data["MessagePriority"] = options.MessagePriority;
                }

                if (!string.IsNullOrEmpty(options.NotificationSound))
                {
                    data["NotificationSound"] = options.NotificationSound;
                }

                var body = options.GetCompiledTemplate()(data);
                _logger.LogDebug("SendAsync Body: {@body}", body);
                using var response = await _httpClientFactory
                    .CreateClient(NamedClient.Default)
                    .PostAsJsonAsync(options.WebhookUri ?? PushoverOption.ApiUrl, body, _jsonSerializerOptions);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                _logger.LogWarning(e, "Error sending notification.");
            }
        }
    }
}