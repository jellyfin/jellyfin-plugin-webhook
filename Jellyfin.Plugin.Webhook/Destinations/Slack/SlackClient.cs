using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations.Pushover;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations.Slack
{
    /// <inheritdoc />
    public class SlackClient : IWebhookClient<SlackOption>
    {
        private readonly ILogger<SlackClient> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlackClient"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="ILogger{SlackClient}"/> interface.</param>
        /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/>.</param>
        public SlackClient(ILogger<SlackClient> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        /// <inheritdoc />
        public async Task SendAsync(SlackOption option, Dictionary<string, object> data)
        {
            try
            {
                data["SlackUsername"] = option.Username;
                data["SlackIconUrl"] = option.IconUrl;

                var body = option.GetCompiledTemplate()(data);
                _logger.LogDebug("SendAsync Body: {@Body}", body);
                using var content = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json);
                using var response = await _httpClientFactory
                    .CreateClient(NamedClient.Default)
                    .PostAsync(option.WebhookUri, content);
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