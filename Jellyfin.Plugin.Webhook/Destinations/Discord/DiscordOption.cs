namespace Jellyfin.Plugin.Webhook.Destinations.Discord;

/// <summary>
/// Discord specific options.
/// </summary>
public class DiscordOption : BaseOption
{
    /// <summary>
    /// Gets or sets the embed color.
    /// </summary>
    public string? EmbedColor { get; set; }

    /// <summary>
    /// Gets or sets the avatar url.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets the bot username.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the mention type.
    /// </summary>
    public DiscordMentionType MentionType { get; set; }
}
