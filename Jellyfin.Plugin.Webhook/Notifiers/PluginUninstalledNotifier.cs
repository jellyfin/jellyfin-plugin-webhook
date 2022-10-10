using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Events.Updates;

namespace Jellyfin.Plugin.Webhook.Notifiers;

/// <summary>
/// Plugin uninstalled notifier.
/// </summary>
public class PluginUninstalledNotifier : IEventConsumer<PluginUninstalledEventArgs>
{
    private readonly IServerApplicationHost _applicationHost;
    private readonly IWebhookSender _webhookSender;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginUninstalledNotifier"/> class.
    /// </summary>
    /// <param name="applicationHost">Instance of the <see cref="IServerApplicationHost"/> interface.</param>
    /// <param name="webhookSender">Instance of the <see cref="IWebhookSender"/> interface.</param>
    public PluginUninstalledNotifier(
        IServerApplicationHost applicationHost,
        IWebhookSender webhookSender)
    {
        _applicationHost = applicationHost;
        _webhookSender = webhookSender;
    }

    /// <inheritdoc />
    public async Task OnEvent(PluginUninstalledEventArgs eventArgs)
    {
        var dataObject = DataObjectHelpers
            .GetBaseDataObject(_applicationHost, NotificationType.PluginUninstalled);
        dataObject["PluginId"] = eventArgs.Argument.Id;
        dataObject["PluginName"] = eventArgs.Argument.Name;
        dataObject["PluginDescription"] = eventArgs.Argument.Description;
        dataObject["PluginVersion"] = eventArgs.Argument.Version;
        dataObject["PluginStatus"] = eventArgs.Argument.Status;

        await _webhookSender.SendNotification(NotificationType.PluginUninstalled, dataObject)
            .ConfigureAwait(false);
    }
}
