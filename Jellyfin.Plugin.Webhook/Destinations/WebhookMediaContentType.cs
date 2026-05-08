namespace Jellyfin.Plugin.Webhook.Destinations;

/// <summary>
/// The type of notification.
/// </summary>
public enum WebhookMediaContentType
{
    /// <summary>
    /// Plaintext/Generic Encoding.
    /// </summary>
    PlainText = 0,

    /// <summary>
    /// JSON Encoded webhook payload.
    /// </summary>
    Json = 1,

    /// <summary>
    /// XML Encoded Webhook Payload.
    /// </summary>
    Xml = 2,
}
