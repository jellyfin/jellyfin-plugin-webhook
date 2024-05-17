using System;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Events.Authentication;

namespace Jellyfin.Plugin.Webhook.Notifiers;

/// <summary>
/// Authentication failure notifier.
/// </summary>
public class AuthenticationFailureNotifier : IEventConsumer<AuthenticationRequestEventArgs>
{
    private readonly IServerApplicationHost _applicationHost;
    private readonly IWebhookSender _webhookSender;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationFailureNotifier"/> class.
    /// </summary>
    /// <param name="applicationHost">Instance of the <see cref="IServerApplicationHost"/> interface.</param>
    /// <param name="webhookSender">Instance of the <see cref="IWebhookSender"/> interface.</param>
    public AuthenticationFailureNotifier(
        IServerApplicationHost applicationHost,
        IWebhookSender webhookSender)
    {
        _applicationHost = applicationHost;
        _webhookSender = webhookSender;
    }

    /// <inheritdoc />
    public async Task OnEvent(AuthenticationRequestEventArgs eventArgs)
    {
        if (eventArgs is null)
        {
            return;
        }

        var dataObject = DataObjectHelpers
            .GetBaseDataObject(_applicationHost, NotificationType.AuthenticationFailure);
        dataObject[nameof(eventArgs.App)] = eventArgs.App ?? string.Empty;
        dataObject[nameof(eventArgs.Username)] = eventArgs.Username ?? string.Empty;
        dataObject[nameof(eventArgs.UserId)] = eventArgs.UserId ?? Guid.Empty;
        dataObject[nameof(eventArgs.AppVersion)] = eventArgs.AppVersion ?? string.Empty;
        dataObject[nameof(eventArgs.DeviceId)] = eventArgs.DeviceId ?? string.Empty;
        dataObject[nameof(eventArgs.DeviceName)] = eventArgs.DeviceName ?? string.Empty;

        await _webhookSender.SendNotification(NotificationType.AuthenticationFailure, dataObject)
            .ConfigureAwait(false);
    }
}
