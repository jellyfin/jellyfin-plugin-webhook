using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Extensions;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations.Bark;

/// <summary>
/// Client for the <see cref="BarkOption"/>.
/// https://github.com/Finb/bark receiver client.
/// https://github.com/Finb/bark-server push server.
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
    private Dictionary<string, object> MergeOptionToData(BarkOption option, Dictionary<string, object> data)
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
            wrappedData["group"] = option.Group;
        }

        if (option.IsArchive is not null)
        {
            wrappedData["isArchive"] = option.IsArchive;
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
