using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Extensions;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations.Discord
{
    /// <inheritdoc />
    public class DiscordClient : IWebhookClient<DiscordOption>
    {
        private readonly ILogger<DiscordClient> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordClient"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="ILogger{DiscordDestination}"/> interface.</param>
        /// <param name="httpClientFactory">Instance of the<see cref="IHttpClientFactory"/> interface.</param>
        public DiscordClient(ILogger<DiscordClient> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        /// <inheritdoc />
        public async Task SendAsync(DiscordOption options, Dictionary<string, object> data)
        {
            try
            {
                if (string.IsNullOrEmpty(options.WebhookUri))
                {
                    throw new ArgumentException(nameof(options.WebhookUri));
                }

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

                // Add discord specific properties.
                data["MentionType"] = GetMentionType(options.MentionType);
                if (!string.IsNullOrEmpty(options.EmbedColor))
                {
                    data["EmbedColor"] = FormatColorCode(options.EmbedColor);
                }

                if (!string.IsNullOrEmpty(options.AvatarUrl))
                {
                    data["AvatarUrl"] = options.AvatarUrl;
                }

                if (!string.IsNullOrEmpty(options.Username))
                {
                    data["Username"] = options.Username;
                }

                var body = options.GetMessageBody(data);
                _logger.LogDebug("SendAsync Body: {@Body}", body);
                using var content = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json);
                using var response = await _httpClientFactory
                    .CreateClient(NamedClient.Default)
                    .PostAsync(new Uri(options.WebhookUri), content)
                    .ConfigureAwait(false);
                await response.LogIfFailedAsync(_logger).ConfigureAwait(false);
            }
            catch (HttpRequestException e)
            {
                _logger.LogWarning(e, "Error sending notification");
            }
        }

        private static int FormatColorCode(string hexCode)
        {
            return int.Parse(hexCode.Substring(1, 6), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        private static string GetMentionType(DiscordMentionType mentionType)
        {
            return mentionType switch
            {
                DiscordMentionType.Everyone => "@everyone",
                DiscordMentionType.Here => "@here",
                _ => string.Empty
            };
        }
    }
}
