using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations.Pushover
{
    /// <inheritdoc />
    public class PushoverClient : IWebhookClient<PushoverOption>
    {
        private readonly ILogger<PushoverClient> _logger;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="PushoverClient"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="ILogger{PushoverClient}"/> interface.</param>
        /// <param name="httpClient">Instance of the <see cref="HttpClient"/>.</param>
        public PushoverClient(ILogger<PushoverClient> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        /// <inheritdoc />
        public async Task SendAsync(PushoverOption options, Dictionary<string, object> data)
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

                if (options.MessagePriority.HasValue)
                {
                    data["MessagePriority"] = options.MessagePriority;
                }

                if (!string.IsNullOrEmpty(options.NotificationSound))
                {
                    data["NotificationSound"] = options.NotificationSound;
                }

                var body = options.GetCompiledTemplate()(data);
                _logger.LogDebug("SendAsync Body: {body}", body);
                using var content = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json);
                using var response = await _httpClient
                    .PostAsync(string.IsNullOrEmpty(options.WebhookUri) ? PushoverOption.ApiUrl : options.WebhookUri, content);
                if (!response.IsSuccessStatusCode)
                {
                    var responseStr = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error sending notification: {Response}", responseStr);
                }
            }
            catch (HttpRequestException e)
            {
                _logger.LogWarning(e, "Error sending notification.");
            }
        }
    }
}