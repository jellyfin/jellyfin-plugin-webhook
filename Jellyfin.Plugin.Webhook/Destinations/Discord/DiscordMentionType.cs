namespace Jellyfin.Plugin.Webhook.Destinations.Discord;

/// <summary>
/// Discord mention type.
/// </summary>
public enum DiscordMentionType
{
    /// <summary>
    /// Mention @everyone.
    /// </summary>
    Everyone = 2,

    /// <summary>
    /// Mention @here.
    /// </summary>
    Here = 1,

    /// <summary>
    /// Mention none.
    /// </summary>
    None = 0
}
