namespace Jellyfin.Plugin.Webhook.Destinations.Slack;

/// <summary>
/// Slack specific options.
/// </summary>
public class SlackOption : BaseOption
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the icon url.
    /// </summary>
    public string IconUrl { get; set; } = string.Empty;
}
