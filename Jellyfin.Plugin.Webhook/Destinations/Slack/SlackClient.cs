using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Extensions;
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
        public async Task SendAsync(SlackOption options, Dictionary<string, object> data)
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

                data["SlackUsername"] = options.Username;
                data["BotUsername"] = options.Username;
                data["SlackIconUrl"] = options.IconUrl;

                var body = options.GetMessageBody(data);
                _logger.LogDebug("SendAsync Body: {@Body}", body);
                using var content = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json);
                using var response = await _httpClientFactory
                    .CreateClient(NamedClient.Default)
                    .PostAsync(new Uri(options.WebhookUri), content)
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
