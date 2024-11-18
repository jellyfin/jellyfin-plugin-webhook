using System;
using System.Collections.Generic;
using System.Text.Json;
using HandlebarsDotNet;
using Jellyfin.Extensions.Json;
using Jellyfin.Plugin.Webhook.Helpers;

namespace Jellyfin.Plugin.Webhook.Destinations;

/// <summary>
/// Base options for destination.
/// </summary>
public abstract class BaseOption
{
    /// <summary>
    /// The data fields that have historically been manually escaped in <see cref="DataObjectHelpers"/>.
    /// More fields may be added as needed.
    /// </summary>
    private readonly List<string> fieldsToEscape =
    [
        "ServerName",
        "Name",
        "Overview",
        "Tagline",
        "ItemType",
        "SeriesName",
        "NotificationUsername",
        "Client",
        "DeviceName",
        "PluginName",
        "PluginChangelog",
        "ExceptionMessage"
    ];

    private HandlebarsTemplate<object, string>? _compiledTemplate;

    /// <summary>
    /// Gets or sets the notification type.
    /// </summary>
    public NotificationType[] NotificationTypes { get; set; } = Array.Empty<NotificationType>();

    /// <summary>
    /// Gets or sets the webhook name.
    /// </summary>
    /// <remarks>
    /// Only used for display.
    /// </remarks>
    public string? WebhookName { get; set; }

    /// <summary>
    /// Gets or sets the webhook uri.
    /// </summary>
    public string? WebhookUri { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to notify on movies.
    /// </summary>
    public bool EnableMovies { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to notify on episodes.
    /// </summary>
    public bool EnableEpisodes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to notify on series.
    /// </summary>
    public bool EnableSeries { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to notify on seasons.
    /// </summary>
    public bool EnableSeasons { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to notify on albums.
    /// </summary>
    public bool EnableAlbums { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to notify on songs.
    /// </summary>
    public bool EnableSongs { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to notify on videos.
    /// </summary>
    public bool EnableVideos { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to send all possible properties.
    /// </summary>
    public bool SendAllProperties { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to trim the message body before sending.
    /// </summary>
    public bool TrimWhitespace { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to skip sending an empty message body.
    /// </summary>
    public bool SkipEmptyMessageBody { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to Enable or Disable Webhook.
    /// </summary>
    public bool EnableWebhook { get; set; } = true;

    /// <summary>
    /// Gets or sets the handlebars template.
    /// </summary>
    public string? Template { get; set; }

    /// <summary>
    /// Gets or sets the webhook user filter.
    /// </summary>
    public Guid[] UserFilter { get; set; } = Array.Empty<Guid>();

    /// <summary>
    /// Gets or sets the Media-Content type of the webhook body. Should not be set by User except for Generic Client.
    /// </summary>
    public WebhookMediaContentType MediaContentType { get; set; } = WebhookMediaContentType.Json;

    /// <summary>
    /// Gets the compiled handlebars template.
    /// </summary>
    /// <returns>The compiled handlebars template.</returns>
    public HandlebarsTemplate<object, string> GetCompiledTemplate()
    {
        return _compiledTemplate ??= Handlebars.Compile(HandlebarsFunctionHelpers.Base64Decode(Template));
    }

    /// <summary>
    /// Gets the message body.
    /// </summary>
    /// <param name="data">The notification body.</param>
    /// <returns>The string message body.</returns>
    public string GetMessageBody(Dictionary<string, object> data)
    {
        Dictionary<string, object> cloneData = new(data); // Quickly clone the dictionary to avoid "over escaping" if we use the same data object for multiple services in a row.
        foreach (var field in fieldsToEscape)
        {
            if (cloneData.TryGetValue(field, out object? value))
            {
                cloneData[field] = MediaContentType switch
                {
                    WebhookMediaContentType.Json => JsonEncode(value.ToString() ?? string.Empty),
                    WebhookMediaContentType.PlainText => DefaultEscape(value.ToString() ?? string.Empty),
                    WebhookMediaContentType.Xml => DefaultEscape(value.ToString() ?? string.Empty),
                    _ => DefaultEscape(value.ToString() ?? string.Empty)
                };
            }
        }

        var body = SendAllProperties
            ? JsonSerializer.Serialize(cloneData, JsonDefaults.Options)
            : GetCompiledTemplate()(cloneData);

        return TrimWhitespace ? body.Trim() : body;
    }

    /// <summary>
    /// Escape a text string using the default Json Encoder.
    /// </summary>
    /// <param name="value">A plain-text string that needs escaped.</param>
    /// <returns>The escaped string.</returns>
    private static string JsonEncode(string value)
    {
        return JsonEncodedText.Encode(value).Value;
    }

    /// <summary>
    /// Escapes all backslashes in a string. This is the previous escape method used for all strings.
    /// </summary>
    /// <param name="value">A plain-text string that needs escaped.</param>
    /// <returns>The escaped string.</returns>
    private static string DefaultEscape(string value)
    {
        return value.Replace("\"", "\\\"", StringComparison.Ordinal);
    }
}
