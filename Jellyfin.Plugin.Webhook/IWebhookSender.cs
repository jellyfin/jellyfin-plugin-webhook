using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations;

namespace Jellyfin.Plugin.Webhook;

/// <summary>
/// Webhook sender interface.
/// </summary>
public interface IWebhookSender
{
    /// <summary>
    /// Send notification with item type.
    /// </summary>
    /// <param name="notificationType">The notification type.</param>
    /// <param name="itemData">The item data.</param>
    /// <param name="itemType">The item type. Default <c>null</c>.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SendNotification(NotificationType notificationType, Dictionary<string, object> itemData, Type? itemType = null);
}
