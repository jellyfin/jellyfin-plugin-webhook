using Jellyfin.Plugin.Webhook.Destinations;

namespace Jellyfin.Plugin.Webhook.Tester
{
    /// <summary>
    /// Configuration for testing webhooks.
    /// </summary>
    public class TestWebhooksDto
    {
        /// <summary>
        /// Gets or sets the notification type.
        /// </summary>
        public required NotificationType NotificationType { get; set; }

        /// <summary>
        /// Gets or sets the item type.
        /// </summary>
        public string? ItemType { get; set; }

        /// <summary>
        /// Gets or sets the test data.
        /// </summary>
        public string? TestData { get; set; }
    }
}
