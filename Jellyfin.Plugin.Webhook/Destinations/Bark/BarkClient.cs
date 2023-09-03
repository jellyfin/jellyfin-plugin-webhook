using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Extensions;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Jellyfin.Plugin.Webhook.Destinations.Bark;

/// <summary>
/// Client for the <see cref="BarkOption"/>.
/// </summary>
public class BarkClient : BaseClient, IWebhookClient<BarkOption>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BarkClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BarkClient"/> class.
    /// </summary>
    /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/>.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{BarkClient}"/> interface.</param>
    public BarkClient(
        IHttpClientFactory httpClientFactory,
        ILogger<BarkClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// MergeOptionToData
    /// merge option properties to data dict.
    /// </summary>
    /// <param name="option">the bark option.</param>
    /// <param name="data">the event data.</param>
    /// <returns>merged data.</returns>
    public Dictionary<string, object> MergeOptionToData(BarkOption option, Dictionary<string, object> data)
    {
        option ??= new BarkOption();
        var wrappedData = new Dictionary<string, object>
        {
            ["data"] = data
        };

        if (!string.IsNullOrWhiteSpace(option.Level))
        {
            wrappedData["level"] = option.Level;
        }

        if (!string.IsNullOrEmpty(option.Icon))
        {
            wrappedData["icon"] = option.Icon;
        }

        if (!string.IsNullOrEmpty(option.Group))
        {
            wrappedData["isArchive"] = option.Group;
        }

        if (!string.IsNullOrEmpty(option.JumpUrl))
        {
            wrappedData["url"] = option.JumpUrl;
        }

        return wrappedData;
    }

    /// <inheritdoc />
    public async Task SendAsync(BarkOption option, Dictionary<string, object> data)
    {
        try
        {
            if (!SendWebhook(_logger, option, data))
            {
                return;
            }

            data = MergeOptionToData(option, data);
            // generate general title and body for bark
            if (option.SendAllProperties)
            {
                data["title"] = "Jellyfin Raw Event";
                data["body"] = JsonSerializer.Serialize(data["data"], new JsonSerializerOptions { WriteIndented = true });
            }

            var body = option.GetMessageBody(data);
            _logger.LogDebug("SendAsync Body: {@Body}", body);
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, option.WebhookUri);
            foreach (var header in option.Headers)
            {
                if (string.IsNullOrEmpty(header.Key) || string.IsNullOrEmpty(header.Value))
                {
                    continue;
                }

                // should not change content type via manually set headers
                if (string.Equals(HeaderNames.ContentType, header.Key, StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrEmpty(header.Value))
                {
                    _logger.LogDebug("should not change content type via manually set headers");
                }
                else
                {
                    httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            httpRequestMessage.Content = new StringContent(body, Encoding.UTF8, "application/json");
            using var response = await _httpClientFactory
                .CreateClient(NamedClient.Default)
                .SendAsync(httpRequestMessage)
                .ConfigureAwait(false);
            await response.LogIfFailedAsync(_logger).ConfigureAwait(false);
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning(e, "Error sending notification");
        }
    }
}
