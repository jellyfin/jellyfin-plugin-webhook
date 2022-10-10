using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations;

/// <summary>
/// The base destination.
/// </summary>
public class BaseClient
{
    /// <summary>
    /// Determines whether the client should send the webhook.
    /// </summary>
    /// <param name="logger">Instance of the <see cref="ILogger"/> interface.</param>
    /// <param name="option">The sender option.</param>
    /// <param name="data">The webhook data.</param>
    /// <returns>Whether the client should send the webhook.</returns>
    protected bool SendWebhook(
        ILogger logger,
        BaseOption option,
        Dictionary<string, object> data)
    {
        var notificationType = data[nameof(NotificationType)] as NotificationType? ?? NotificationType.None;

        // Don't filter on UserId if the notification type is UserCreated.
        if (notificationType is not NotificationType.UserCreated
            && option.UserFilter.Length is not 0
            && data.TryGetValue("UserId", out var userIdObj)
            && userIdObj is Guid userId
            && Array.IndexOf(option.UserFilter, userId) is -1)
        {
            logger.LogDebug("UserId {UserId} not found in user filter, ignoring event", userId);
            return false;
        }

        return true;
    }
}
