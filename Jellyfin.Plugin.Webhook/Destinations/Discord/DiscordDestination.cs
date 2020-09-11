using System.Collections.Generic;
using System.Globalization;
using System.Net.Mime;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations.Discord
{
    /// <inheritdoc />
    public class DiscordDestination : IDestination<DiscordOption>
    {
        private readonly ILogger<DiscordDestination> _logger;
        private readonly IHttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordDestination"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="ILogger{DiscordDestination}"/> interface.</param>
        /// <param name="httpClient">Instance of the <see cref="IHttpClient"/> interface.</param>
        public DiscordDestination(ILogger<DiscordDestination> logger, IHttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        /// <inheritdoc />
        public async Task SendAsync(DiscordOption options, Dictionary<string, object> data)
        {
            try
            {
                // Add discord specific properties.
                data["MentionType"] = GetMentionType(options.MentionType);
                data["EmbedColor"] = FormatColorCode(options.EmbedColor);
                data["AvatarUrl"] = options.AvatarUrl;
                data["Username"] = options.Username;

                var body = options.GetCompiledTemplate()(data);
                _logger.LogTrace("SendAsync Body: {body}", body);
                var requestOptions = new HttpRequestOptions
                {
                    Url = options.WebhookUri,
                    RequestContent = body,
                    RequestContentType = MediaTypeNames.Application.Json
                };

                using var response = await _httpClient.Post(requestOptions).ConfigureAwait(false);
            }
            catch (HttpException e)
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