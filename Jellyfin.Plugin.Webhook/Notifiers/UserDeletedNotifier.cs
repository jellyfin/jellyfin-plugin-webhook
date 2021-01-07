using System.Threading.Tasks;
using Jellyfin.Data.Events.Users;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Common;
using MediaBrowser.Controller.Events;

namespace Jellyfin.Plugin.Webhook.Notifiers
{
    /// <summary>
    /// User deleted notifier.
    /// </summary>
    public class UserDeletedNotifier : IEventConsumer<UserDeletedEventArgs>
    {
        private readonly IApplicationHost _applicationHost;
        private readonly IWebhookSender _webhookSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDeletedNotifier"/> class.
        /// </summary>
        /// <param name="applicationHost">Instance of the <see cref="IApplicationHost"/> interface.</param>
        /// <param name="webhookSender">Instance of the <see cref="IWebhookSender"/> interface.</param>
        public UserDeletedNotifier(
            IApplicationHost applicationHost,
            IWebhookSender webhookSender)
        {
            _applicationHost = applicationHost;
            _webhookSender = webhookSender;
        }

        /// <inheritdoc />
        public async Task OnEvent(UserDeletedEventArgs eventArgs)
        {
            var dataObject = DataObjectHelpers
                .GetBaseDataObject(_applicationHost, NotificationType.UserDeleted)
                .AddUserData(eventArgs.Argument);

            await _webhookSender.SendNotification(NotificationType.UserDeleted, dataObject);
        }
    }
}