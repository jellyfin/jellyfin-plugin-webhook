namespace Jellyfin.Plugin.Webhook.Destinations
{
    /// <summary>
    /// The type of notification.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Item added notification.
        /// </summary>
        ItemAdded = 1,

        /// <summary>
        /// Generic notification.
        /// </summary>
        Generic = 2,

        /// <summary>
        /// Playback start notification.
        /// </summary>
        PlaybackStart = 3,
    }
}