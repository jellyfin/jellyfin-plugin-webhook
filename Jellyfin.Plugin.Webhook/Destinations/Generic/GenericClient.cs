using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Jellyfin.Plugin.Webhook.Destinations.Generic
{
    /// <inheritdoc />
    public class GenericClient : IWebhookClient<GenericOption>
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GenericClient> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericClient"/> class.
        /// </summary>
        /// <param name="httpClient">Instance of the <see cref="HttpClient"/>.</param>
        /// <param name="logger">Instance of the <see cref="ILogger{GenericClient}"/> interface.</param>
        public GenericClient(
            HttpClient httpClient,
            ILogger<GenericClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task SendAsync(GenericOption options, Dictionary<string, object> data)
        {
            try
            {
                foreach (var field in options.Fields)
                {
                    if (string.IsNullOrEmpty(field.Key) || string.IsNullOrEmpty(field.Value))
                    {
                        continue;
                    }

                    data[field.Key] = field.Value;
                }

                var body = options.GetCompiledTemplate()(data);
                _logger.LogDebug("SendAsync Body: {@Body}", body);
                using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, options.WebhookUri);
                var contentType = MediaTypeNames.Text.Plain;
                foreach (var header in options.Headers)
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
                using var response = await _httpClient.SendAsync(httpRequestMessage);
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