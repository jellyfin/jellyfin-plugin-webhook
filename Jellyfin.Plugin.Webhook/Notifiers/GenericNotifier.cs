using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Entities;
using Jellyfin.Plugin.Webhook.Destinations;
using MediaBrowser.Controller.Notifications;

namespace Jellyfin.Plugin.Webhook.Notifiers
{
    /// <summary>
    /// Generic notifier.
    /// </summary>
    public class GenericNotifier : INotificationService
    {
        private readonly WebhookSender _webhookSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericNotifier"/> class.
        /// </summary>
        /// <param name="webhookSender">Instance of the <see cref="WebhookSender"/>.</param>
        public GenericNotifier(WebhookSender webhookSender)
        {
            _webhookSender = webhookSender;
        }

        /// <inheritdoc />
        public string Name => "Webhook: Generic Notifier";

        /// <inheritdoc />
        public async Task SendNotification(UserNotification request, CancellationToken cancellationToken)
        {
            var parameters = new Dictionary<string, object>
            {
                { nameof(request.Name), request.Name },
                { nameof(request.Description), request.Description },
                { nameof(request.Date), request.Date },
                { nameof(request.Level), request.Level },
                { nameof(request.Url), request.Url },
                { nameof(request.User.Username), request.User.Username },
                { "UserId", request.User.Id },
            };

            await _webhookSender.SendGenericNotification(NotificationType.Generic, parameters);
        }

        /// <inheritdoc />
        public bool IsEnabledForUser(User user) => true;
    }
}