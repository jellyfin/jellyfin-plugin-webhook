using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MediaBrowser.Common.Json;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations.Generic
{
    /// <inheritdoc />
    public class GenericClient : IWebhookClient<GenericOption>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GenericClient> _logger;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericClient"/> class.
        /// </summary>
        /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/> interface.</param>
        /// <param name="logger">Instance of the <see cref="ILogger{GenericClient}"/> interface.</param>
        public GenericClient(
            IHttpClientFactory httpClientFactory,
            ILogger<GenericClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _jsonSerializerOptions = JsonDefaults.GetOptions();
        }

        /// <inheritdoc />
        public async Task SendItemAddedAsync(GenericOption options, Dictionary<string, object> data)
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
                _logger.LogDebug("SendAsync Body: {@body}", body);
                using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, options.WebhookUri);
                foreach (var header in options.Headers)
                {
                    if (string.IsNullOrEmpty(header.Key) || string.IsNullOrEmpty(header.Value))
                    {
                        continue;
                    }

                    httpRequestMessage.Headers.TryAddWithoutValidation(header.Value, header.Value);
                }

                var jsonString = JsonSerializer.Serialize(body, _jsonSerializerOptions);
                httpRequestMessage.Content = new StringContent(jsonString, Encoding.UTF8, MediaTypeNames.Application.Json);
                using var response = await _httpClientFactory
                    .CreateClient(NamedClient.Default)
                    .SendAsync(httpRequestMessage);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                _logger.LogWarning(e, "Error sending notification.");
            }
        }
    }
}