using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Entities;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Common;
using MediaBrowser.Controller.Notifications;

namespace Jellyfin.Plugin.Webhook.Notifiers
{
    /// <summary>
    /// Generic notifier.
    /// </summary>
    public class GenericNotifier : INotificationService
    {
        private readonly IWebhookSender _webhookSender;
        private readonly IApplicationHost _applicationHost;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericNotifier"/> class.
        /// </summary>
        /// <param name="webhookSender">Instance of the <see cref="IWebhookSender"/> interface.</param>
        /// <param name="applicationHost">Instance of the <see cref="IApplicationHost"/> interface.</param>
        public GenericNotifier(IWebhookSender webhookSender, IApplicationHost applicationHost)
        {
            _webhookSender = webhookSender;
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

            await _webhookSender.SendNotification(NotificationType.Generic, dataObject)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public bool IsEnabledForUser(User user) => true;
    }
}