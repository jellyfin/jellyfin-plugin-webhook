using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Extensions;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Jellyfin.Plugin.Webhook.Destinations.Generic;

/// <summary>
/// Client for the <see cref="GenericOption"/>.
/// </summary>
public class GenericClient : BaseClient, IWebhookClient<GenericOption>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GenericClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericClient"/> class.
    /// </summary>
    /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/>.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{GenericClient}"/> interface.</param>
    public GenericClient(
        IHttpClientFactory httpClientFactory,
        ILogger<GenericClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendAsync(GenericOption option, Dictionary<string, object> data)
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

            _logger.LogDebug("SendAsync Body: {@Body}", body);
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, option.WebhookUri);
            var contentType = MediaTypeNames.Text.Plain;
            foreach (var header in option.Headers)
            {
                if (string.IsNullOrEmpty(header.Key) || string.IsNullOrEmpty(header.Value))
                {
                    continue;
                }

                // Content-Type cannot be set manually, must be set on the content.
                if (string.Equals(HeaderNames.ContentType, header.Key, StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrEmpty(header.Value))
                {
                    contentType = header.Value;
                }
                else
                {
                    httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            httpRequestMessage.Content = new StringContent(body, Encoding.UTF8, contentType);
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
