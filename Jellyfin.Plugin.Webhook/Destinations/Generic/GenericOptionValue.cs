namespace Jellyfin.Plugin.Webhook.Destinations.Generic;

/// <summary>
/// Generic option value.
/// </summary>
public class GenericOptionValue
{
    /// <summary>
    /// Gets or sets the option key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the option value.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
