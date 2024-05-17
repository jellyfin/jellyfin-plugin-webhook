using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Events.Authentication;

namespace Jellyfin.Plugin.Webhook.Notifiers;

/// <summary>
/// Authentication success notifier.
/// </summary>
public class AuthenticationSuccessNotifier : IEventConsumer<AuthenticationResultEventArgs>
{
    private readonly IServerApplicationHost _applicationHost;
    private readonly IWebhookSender _webhookSender;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationSuccessNotifier"/> class.
    /// </summary>
    /// <param name="applicationHost">Instance of the <see cref="IServerApplicationHost"/> interface.</param>
    /// <param name="webhookSender">Instance of the <see cref="IWebhookSender"/> interface.</param>
    public AuthenticationSuccessNotifier(
        IServerApplicationHost applicationHost,
        IWebhookSender webhookSender)
    {
        _applicationHost = applicationHost;
        _webhookSender = webhookSender;
    }

    /// <inheritdoc />
    public async Task OnEvent(AuthenticationResultEventArgs eventArgs)
    {
        if (eventArgs is null)
        {
            return;
        }

        var dataObject = DataObjectHelpers
            .GetBaseDataObject(_applicationHost, NotificationType.AuthenticationSuccess)
            .AddUserData(eventArgs.User);

        await _webhookSender.SendNotification(NotificationType.AuthenticationSuccess, dataObject)
            .ConfigureAwait(false);
    }
}
