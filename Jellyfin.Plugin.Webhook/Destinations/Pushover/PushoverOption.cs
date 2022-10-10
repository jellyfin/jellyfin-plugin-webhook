using System;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Webhook.Destinations.Pushover;

/// <summary>
/// Pushover specific options.
/// </summary>
public class PushoverOption : BaseOption
{
    /// <summary>
    /// The webhook endpoint.
    /// </summary>
    [JsonIgnore]
    public static readonly Uri ApiUrl = new Uri("https://api.pushover.net/1/messages.json");

    /// <summary>
    /// Gets or sets the pushover token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pushover user token.
    /// </summary>
    public string UserToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the device.
    /// </summary>
    public string? Device { get; set; }

    /// <summary>
    /// Gets or sets the message title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the message url.
    /// </summary>
    public string? MessageUrl { get; set; }

    /// <summary>
    /// Gets or sets the message url title.
    /// </summary>
    public string? MessageUrlTitle { get; set; }

    /// <summary>
    /// Gets or sets the message priority.
    /// </summary>
    public int? MessagePriority { get; set; }

    /// <summary>
    /// Gets or sets the notification sound.
    /// </summary>
    public string? NotificationSound { get; set; }
}
