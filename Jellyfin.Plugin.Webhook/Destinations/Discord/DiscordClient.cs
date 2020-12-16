using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MediaBrowser.Common.Json;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations.Discord
{
    /// <inheritdoc />
    public class DiscordClient : IWebhookClient<DiscordOption>
    {
        private readonly ILogger<DiscordClient> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordClient"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="ILogger{DiscordDestination}"/> interface.</param>
        /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/> interface.</param>
        public DiscordClient(ILogger<DiscordClient> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _jsonSerializerOptions = JsonDefaults.GetOptions();
        }

        /// <inheritdoc />
        public async Task SendItemAddedAsync(DiscordOption options, Dictionary<string, object> data)
        {
            try
            {
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

                var body = options.GetCompiledTemplate()(data);
                _logger.LogDebug("SendAsync Body: {@body}", body);
                using var response = await _httpClientFactory
                    .CreateClient(NamedClient.Default)
                    .PostAsJsonAsync(options.WebhookUri, body, _jsonSerializerOptions);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                _logger.LogWarning(e, "Error sending notification.");
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