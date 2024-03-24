using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Extensions;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations.Pushbullet;

/// <summary>
/// Client for the <see cref="PushbulletOption"/>.
/// </summary>
public class PushbulletClient : BaseClient, IWebhookClient<PushbulletOption>
{
    private readonly ILogger<PushbulletClient> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PushbulletClient"/> class.
    /// </summary>
    /// <param name="logger">Instance of the <see cref="ILogger{PushbulletClient}"/> interface.</param>
    /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/> interface.</param>
    public PushbulletClient(
        ILogger<PushbulletClient> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc />
    public async Task SendAsync(PushbulletOption option, Dictionary<string, object> data)
    {
        try
        {
            if (!SendWebhook(_logger, option, data))
            {
                return;
            }

            data["PushbulletToken"] = option.Token;
            data["PushbulletDeviceId"] = option.DeviceId;
            data["PushbulletChannel"] = option.Channel;

            var body = option.GetMessageBody(data);
            if (!SendMessageBody(_logger, option, body))
            {
                return;
            }

            _logger.LogDebug("SendAsync Body: {@Body}", body);

            using var requestOptions = new HttpRequestMessage(HttpMethod.Post, string.IsNullOrEmpty(option.WebhookUri) ? PushbulletOption.ApiUrl : option.WebhookUri);
            requestOptions.Headers.TryAddWithoutValidation("Access-Token", option.Token);
            requestOptions.Content = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json);

            using var response = await _httpClientFactory
                .CreateClient(NamedClient.Default)
                .SendAsync(requestOptions)
                .ConfigureAwait(false);
            await response.LogIfFailedAsync(_logger).ConfigureAwait(false);
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning(e, "Error sending notification");
        }
    }
}
