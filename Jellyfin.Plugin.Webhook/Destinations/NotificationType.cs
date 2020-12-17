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

        /// <summary>
        /// Playback progress notification.
        /// </summary>
        PlaybackProgress = 4,

        /// <summary>
        /// Playback stop notification.
        /// </summary>
        PlaybackStop = 5
    }
}