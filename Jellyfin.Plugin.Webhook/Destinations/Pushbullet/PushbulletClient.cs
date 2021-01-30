using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations.Pushbullet
{
    /// <inheritdoc />
    public class PushbulletClient : IWebhookClient<PushbulletOption>
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
        public async Task SendAsync(PushbulletOption options, Dictionary<string, object> data)
        {
            try
            {
                data["PushbulletToken"] = options.Token;
                data["PushbulletDeviceId"] = options.DeviceId;
                data["PushbulletChannel"] = options.Channel;

                var body = options.GetCompiledTemplate()(data);
                _logger.LogDebug("SendAsync Body: {@Body}", body);

                using var requestOptions = new HttpRequestMessage(HttpMethod.Post, string.IsNullOrEmpty(options.WebhookUri) ? PushbulletOption.ApiUrl : options.WebhookUri);
                requestOptions.Headers.TryAddWithoutValidation("Access-Token", options.Token);
                requestOptions.Content = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json);

                using var response = await _httpClientFactory
                    .CreateClient(NamedClient.Default)
                    .SendAsync(requestOptions);
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