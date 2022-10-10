using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Webhook.Destinations.Pushbullet;

/// <summary>
/// Pushbullet specific option.
/// </summary>
public class PushbulletOption : BaseOption
{
    /// <summary>
    /// The webhook endpoint.
    /// </summary>
    [JsonIgnore]
    public const string ApiUrl = "https://api.pushbullet.com/v2/pushes";

    /// <summary>
    /// Gets or sets the pushbullet token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the device id.
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the channel.
    /// </summary>
    public string Channel { get; set; } = string.Empty;
}
