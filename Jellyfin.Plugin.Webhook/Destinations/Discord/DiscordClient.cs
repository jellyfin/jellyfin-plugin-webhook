using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Extensions;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations.Discord;

/// <summary>
/// Client for the <see cref="DiscordOption"/>.
/// </summary>
public class DiscordClient : BaseClient, IWebhookClient<DiscordOption>
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
    public async Task SendAsync(DiscordOption option, Dictionary<string, object> data)
    {
        if (string.IsNullOrEmpty(option.WebhookUri))
        {
            throw new ArgumentException(nameof(option.WebhookUri));
        }

        if (!SendWebhook(_logger, option, data))
        {
            return;
        }

        // Add discord specific properties.
        data["MentionType"] = GetMentionType(option.MentionType);
        if (!string.IsNullOrEmpty(option.EmbedColor))
        {
            data["EmbedColor"] = FormatColorCode(option.EmbedColor);
        }

        if (!string.IsNullOrEmpty(option.AvatarUrl))
        {
            data["AvatarUrl"] = option.AvatarUrl;
        }

        if (!string.IsNullOrEmpty(option.Username))
        {
            data["Username"] = option.Username;
            data["BotUsername"] = option.Username;
        }

        var body = option.GetMessageBody(data);
        if (!SendMessageBody(_logger, option, body))
        {
            return;
        }

        _logger.LogDebug("SendAsync Body: {@Body}", body);

        // Determine if a thumbnail should be attached
        var attachThumbnail = false;
        try
        {
            foreach (var embedElement in JsonDocument.Parse(body).RootElement.GetProperty("embeds").EnumerateArray())
            {
                if (embedElement.GetProperty("thumbnail").GetProperty("url").GetString() == "attachment://thumbnail.jpg")
                {
                    _logger.LogDebug("Attaching thumbnail");
                    attachThumbnail = true;
                    break;
                }
            }
        }
        catch (Exception e) when (e is JsonException || e is ArgumentException || e is InvalidOperationException || e is KeyNotFoundException)
        {
            _logger.LogDebug(e, "Not attaching thumbnail");
        }

        if (attachThumbnail)
        {
            // Get the thumbnail
            var serverUrl = data.GetValueOrDefault("ServerUrl") as string;
            var itemId = data.GetValueOrDefault("ItemId", Guid.Empty).ToString();
            var thumbnailUrl = serverUrl + "/Items/" + itemId + "/Images/Primary";
            byte[] thumbnailBytes;
            try
            {
                thumbnailBytes = await _httpClientFactory
                    .CreateClient(NamedClient.Default)
                    .GetByteArrayAsync(thumbnailUrl)
                    .ConfigureAwait(false);
            }
            catch (Exception e) when (e is InvalidOperationException || e is HttpRequestException || e is TaskCanceledException || e is UriFormatException)
            {
                _logger.LogWarning(e, "Coudn't get thumbnail from {Url}", thumbnailUrl);
                using var stringContent = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json);
                await SendContent(option.WebhookUri, stringContent).ConfigureAwait(false);
                return;
            }

            // Send the thumbnail and the body as a multipart form
            using var thumbnailContent = new ByteArrayContent(thumbnailBytes);
            using var bodyContent = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json);
            using var content = new MultipartFormDataContent
            {
                { thumbnailContent, "files[0]", "thumbnail.jpg" },
                { bodyContent, "payload_json" }
            };
            await SendContent(option.WebhookUri, content).ConfigureAwait(false);
        }
        else
        {
            using var content = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json);
            await SendContent(option.WebhookUri, content).ConfigureAwait(false);
        }
    }

    private async Task SendContent(string webhookUri, HttpContent content)
    {
        _logger.LogDebug("Sending content as: {Type}", content.GetType());
        try
        {
            using var response = await _httpClientFactory
                .CreateClient(NamedClient.Default)
                .PostAsync(webhookUri, content)
                .ConfigureAwait(false);
            await response.LogIfFailedAsync(_logger).ConfigureAwait(false);
        }
        catch (Exception e) when (e is InvalidOperationException || e is HttpRequestException || e is TaskCanceledException || e is UriFormatException)
        {
            _logger.LogWarning(e, "Error sending notification");
        }
    }

    private static int FormatColorCode(string hexCode)
    {
        return int.Parse(hexCode[1..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
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
