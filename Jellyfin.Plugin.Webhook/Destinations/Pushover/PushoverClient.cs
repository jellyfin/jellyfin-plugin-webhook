using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Extensions;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations.Pushover;

/// <summary>
/// Client for the <see cref="PushoverOption"/>.
/// </summary>
public class PushoverClient : BaseClient, IWebhookClient<PushoverOption>
{
    private readonly ILogger<PushoverClient> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PushoverClient"/> class.
    /// </summary>
    /// <param name="logger">Instance of the <see cref="ILogger{PushoverClient}"/> interface.</param>
    /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/>.</param>
    public PushoverClient(ILogger<PushoverClient> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc />
    public async Task SendAsync(PushoverOption option, Dictionary<string, object> data)
    {
        try
        {
            if (!SendWebhook(_logger, option, data))
            {
                return;
            }

            data["Token"] = option.Token;
            data["UserToken"] = option.UserToken;
            if (!string.IsNullOrEmpty(option.Device))
            {
                data["Device"] = option.Device;
            }

            if (!string.IsNullOrEmpty(option.Title))
            {
                data["Title"] = option.Title;
            }

            if (!string.IsNullOrEmpty(option.MessageUrl))
            {
                data["MessageUrl"] = option.MessageUrl;
            }

            if (!string.IsNullOrEmpty(option.MessageUrlTitle))
            {
                data["MessageUrlTitle"] = option.MessageUrlTitle;
            }

            if (option.MessagePriority is not null)
            {
                data["MessagePriority"] = option.MessagePriority;
            }

            if (!string.IsNullOrEmpty(option.NotificationSound))
            {
                data["NotificationSound"] = option.NotificationSound;
            }

            var body = option.GetMessageBody(data);
            if (!SendMessageBody(_logger, option, body))
            {
                return;
            }

            _logger.LogDebug("SendAsync Body: {@Body}", body);
            using var content = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json);
            using var response = await _httpClientFactory
                .CreateClient(NamedClient.Default)
                .PostAsync(string.IsNullOrEmpty(option.WebhookUri) ? PushoverOption.ApiUrl : new Uri(option.WebhookUri), content)
                .ConfigureAwait(false);
            await response.LogIfFailedAsync(_logger).ConfigureAwait(false);
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning(e, "Error sending notification");
        }
    }
}
