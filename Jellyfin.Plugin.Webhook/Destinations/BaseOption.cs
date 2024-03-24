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
    /// Gets or sets the handlebars template.
    /// </summary>
    public string? Template { get; set; }

    /// <summary>
    /// Gets or sets the webhook user filter.
    /// </summary>
    public Guid[] UserFilter { get; set; } = Array.Empty<Guid>();

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
        var body = SendAllProperties
            ? JsonSerializer.Serialize(data, JsonDefaults.Options)
            : GetCompiledTemplate()(data);

        return TrimWhitespace ? body.Trim() : body;
    }
}
