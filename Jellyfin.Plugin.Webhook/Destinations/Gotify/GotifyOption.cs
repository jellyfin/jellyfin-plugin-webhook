namespace Jellyfin.Plugin.Webhook.Destinations.Gotify;

/// <summary>
/// Gotify specific options.
/// </summary>
public class GotifyOption : BaseOption
{
    /// <summary>
    /// Gets or sets the authentication token.
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Gets or sets the notification priority.
    /// </summary>
    public int Priority { get; set; }
}
