using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Extensions;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations.GenericForm;

/// <summary>
/// Client for the <see cref="GenericFormOption"/>.
/// </summary>
public class GenericFormClient : BaseClient, IWebhookClient<GenericFormOption>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GenericFormClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericFormClient"/> class.
    /// </summary>
    /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/>.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{GenericFormClient}"/> interface.</param>
    public GenericFormClient(
        IHttpClientFactory httpClientFactory,
        ILogger<GenericFormClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendAsync(GenericFormOption option, Dictionary<string, object> data)
    {
        try
        {
            if (!SendWebhook(_logger, option, data))
            {
                return;
            }

            foreach (var field in option.Fields)
            {
                if (string.IsNullOrEmpty(field.Key) || string.IsNullOrEmpty(field.Value))
                {
                    continue;
                }

                data[field.Key] = field.Value;
            }

            var body = option.GetMessageBody(data);
            if (!SendMessageBody(_logger, option, body))
            {
                return;
            }

            var dictionaryBody = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
            if (dictionaryBody is null)
            {
                _logger.LogWarning("Body is null, unable to send webhook");
                return;
            }

            _logger.LogDebug("SendAsync Body: {@Body}", body);
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, option.WebhookUri);
            foreach (var header in option.Headers)
            {
                if (string.IsNullOrEmpty(header.Key) || string.IsNullOrEmpty(header.Value))
                {
                    continue;
                }

                httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            httpRequestMessage.Content = new FormUrlEncodedContent(dictionaryBody!);
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
