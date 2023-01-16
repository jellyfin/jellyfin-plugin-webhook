using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Entities;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Notifications;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.Webhook.Notifiers;

/// <summary>
/// Generic notifier.
/// </summary>
public class GenericNotifier : INotificationService
{
    private readonly IServerApplicationHost _applicationHost;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericNotifier"/> class.
    /// </summary>
    /// <param name="applicationHost">Instance of the <see cref="IServerApplicationHost"/> interface.</param>
    public GenericNotifier(IServerApplicationHost applicationHost)
    {
        _applicationHost = applicationHost;
    }

    /// <inheritdoc />
    public string Name => "Webhook: Generic Notifier";

    /// <inheritdoc />
    public async Task SendNotification(UserNotification request, CancellationToken cancellationToken)
    {
        var dataObject = DataObjectHelpers
            .GetBaseDataObject(_applicationHost, NotificationType.Generic);
        dataObject[nameof(request.Name)] = request.Name;
        dataObject[nameof(request.Description)] = request.Description;
        dataObject[nameof(request.Date)] = request.Date;
        dataObject[nameof(request.Level)] = request.Level;
        dataObject[nameof(request.Url)] = request.Url;
        dataObject[nameof(request.User.Username)] = request.User.Username;
        dataObject["UserId"] = request.User.Id;

        var scope = _applicationHost.ServiceProvider!.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            var webhookSender = scope.ServiceProvider.GetRequiredService<IWebhookSender>();
            await webhookSender.SendNotification(NotificationType.Generic, dataObject)
                .ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public bool IsEnabledForUser(User user) => true;
}
