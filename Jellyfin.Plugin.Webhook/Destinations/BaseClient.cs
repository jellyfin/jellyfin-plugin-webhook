using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Extensions;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations;

/// <summary>
/// The base destination.
/// </summary>
public class BaseClient
{
    /// <summary>
    /// The maximum time to wait for a destination to respond.
    /// </summary>
    private static readonly TimeSpan SendTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Sends the request to the destination, giving up after <see cref="SendTimeout"/>.
    /// </summary>
    /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/>.</param>
    /// <param name="request">The request to send.</param>
    /// <param name="logger">Instance of the <see cref="ILogger"/> interface.</param>
    /// <returns>A task representing the send.</returns>
    protected static async Task SendAsync(
        IHttpClientFactory httpClientFactory,
        HttpRequestMessage request,
        ILogger logger)
    {
        try
        {
            using var cts = new CancellationTokenSource(SendTimeout);
            using var response = await httpClientFactory
                .CreateClient(NamedClient.Default)
                .SendAsync(request, cts.Token)
                .ConfigureAwait(false);
            await response.LogIfFailedAsync(logger).ConfigureAwait(false);
        }
        catch (TaskCanceledException e)
        {
            logger.LogWarning(e, "Timed out sending notification after {Timeout}", SendTimeout);
        }
    }

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

    /// <summary>
    /// Determines whether the client should send the webhook with the given message body.
    /// </summary>
    /// <param name="logger">Instance of the <see cref="ILogger"/> interface.</param>
    /// <param name="option">The sender option.</param>
    /// <param name="body">The message body.</param>
    /// <returns>Whether the client should send the webhook.</returns>
    protected bool SendMessageBody(
        ILogger logger,
        BaseOption option,
        string body)
    {
        if (option.SkipEmptyMessageBody && string.IsNullOrWhiteSpace(body))
        {
            logger.LogDebug("Skip sending empty message body");
            return false;
        }

        return true;
    }
}
