using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Events.Updates;

namespace Jellyfin.Plugin.Webhook.Notifiers;

/// <summary>
/// Plugin installation cancelled.
/// </summary>
public class PluginInstallationCancelledNotifier : IEventConsumer<PluginInstallationCancelledEventArgs>
{
    private readonly IServerApplicationHost _applicationHost;
    private readonly IWebhookSender _webhookSender;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginInstallationCancelledNotifier"/> class.
    /// </summary>
    /// <param name="applicationHost">Instance of the <see cref="IServerApplicationHost"/> interface.</param>
    /// <param name="webhookSender">Instance of the <see cref="IWebhookSender"/> interface.</param>
    public PluginInstallationCancelledNotifier(
        IServerApplicationHost applicationHost,
        IWebhookSender webhookSender)
    {
        _applicationHost = applicationHost;
        _webhookSender = webhookSender;
    }

    /// <inheritdoc />
    public async Task OnEvent(PluginInstallationCancelledEventArgs eventArgs)
    {
        var dataObject = DataObjectHelpers
            .GetBaseDataObject(_applicationHost, NotificationType.PluginInstallationCancelled)
            .AddPluginInstallationInfo(eventArgs.Argument);

        await _webhookSender.SendNotification(NotificationType.PluginInstallationCancelled, dataObject)
            .ConfigureAwait(false);
    }
}
