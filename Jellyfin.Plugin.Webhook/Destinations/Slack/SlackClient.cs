using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations.Slack;

/// <summary>
/// Client for the <see cref="SlackOption"/>.
/// </summary>
public class SlackClient : BaseClient, IWebhookClient<SlackOption>
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
            if (string.IsNullOrEmpty(option.WebhookUri))
            {
                throw new ArgumentException(nameof(option.WebhookUri));
            }

            if (!SendWebhook(_logger, option, data))
            {
                return;
            }

            data["SlackUsername"] = option.Username;
            data["BotUsername"] = option.Username;
            data["SlackIconUrl"] = option.IconUrl;

            var body = option.GetMessageBody(data);
            if (!SendMessageBody(_logger, option, body))
            {
                return;
            }

            _logger.LogDebug("SendAsync Body: {@Body}", body);
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(option.WebhookUri))
            {
                Content = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json)
            };
            await SendAsync(_httpClientFactory, request, _logger).ConfigureAwait(false);
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning(e, "Error sending notification");
        }
    }
}
