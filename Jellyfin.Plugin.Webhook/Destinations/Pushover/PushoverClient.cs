using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

using Jellyfin.Plugin.Webhook.Extensions;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations.Pushover
{
    /// <inheritdoc />
    public class PushoverClient : IWebhookClient<PushoverOption>
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
        public async Task SendAsync(PushoverOption options, Dictionary<string, object> data)
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

                data["Token"] = options.Token;
                data["UserToken"] = options.UserToken;
                if (!string.IsNullOrEmpty(options.Device))
                {
                    data["Device"] = options.Device;
                }

                if (!string.IsNullOrEmpty(options.Title))
                {
                    data["Title"] = options.Title;
                }

                if (!string.IsNullOrEmpty(options.MessageUrl))
                {
                    data["MessageUrl"] = options.MessageUrl;
                }

                if (!string.IsNullOrEmpty(options.MessageUrlTitle))
                {
                    data["MessageUrlTitle"] = options.MessageUrlTitle;
                }

                if (options.MessagePriority is not null)
                {
                    data["MessagePriority"] = options.MessagePriority;
                }

                if (!string.IsNullOrEmpty(options.NotificationSound))
                {
                    data["NotificationSound"] = options.NotificationSound;
                }

                var body = options.GetMessageBody(data);
                _logger.LogDebug("SendAsync Body: {@Body}", body);
                using var content = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json);
                using var response = await _httpClientFactory
                    .CreateClient(NamedClient.Default)
                    .PostAsync(string.IsNullOrEmpty(options.WebhookUri) ? PushoverOption.ApiUrl : new Uri(options.WebhookUri), content)
                    .ConfigureAwait(false);
                await response.LogIfFailed(_logger).ConfigureAwait(false);
            }
            catch (HttpRequestException e)
            {
                _logger.LogWarning(e, "Error sending notification");
            }
        }
    }
}