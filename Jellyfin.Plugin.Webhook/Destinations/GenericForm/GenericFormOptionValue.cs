namespace Jellyfin.Plugin.Webhook.Destinations.GenericForm;

/// <summary>
/// Generic form option value.
/// </summary>
public class GenericFormOptionValue
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
