using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Extensions;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations.GenericForm
{
    /// <summary>
    /// Client for the <see cref="GenericFormOption"/>.
    /// </summary>
    public class GenericFormClient : IWebhookClient<GenericFormOption>
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
        public async Task SendAsync(GenericFormOption options, Dictionary<string, object> data)
        {
            try
            {
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

                foreach (var field in options.Fields)
                {
                    if (string.IsNullOrEmpty(field.Key) || string.IsNullOrEmpty(field.Value))
                    {
                        continue;
                    }

                    data[field.Key] = field.Value;
                }

                var body = options.GetMessageBody(data);
                var dictionaryBody = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
                if (dictionaryBody is null)
                {
                    _logger.LogWarning("Body is null, unable to send webhook");
                    return;
                }

                _logger.LogDebug("SendAsync Body: {@Body}", body);
                using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, options.WebhookUri);
                foreach (var header in options.Headers)
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
}
