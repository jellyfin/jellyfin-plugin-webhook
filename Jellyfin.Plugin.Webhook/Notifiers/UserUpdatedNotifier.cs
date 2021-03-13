using System.Threading.Tasks;
using Jellyfin.Data.Events.Users;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Common;
using MediaBrowser.Controller.Events;

namespace Jellyfin.Plugin.Webhook.Notifiers
{
    /// <summary>
    /// User updated notifier.
    /// </summary>
    public class UserUpdatedNotifier : IEventConsumer<UserUpdatedEventArgs>
    {
        private readonly IApplicationHost _applicationHost;
        private readonly IWebhookSender _webhookSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserUpdatedNotifier"/> class.
        /// </summary>
        /// <param name="applicationHost">Instance of the <see cref="IApplicationHost"/> interface.</param>
        /// <param name="webhookSender">Instance of the <see cref="IWebhookSender"/> interface.</param>
        public UserUpdatedNotifier(
            IApplicationHost applicationHost,
            IWebhookSender webhookSender)
        {
            _applicationHost = applicationHost;
            _webhookSender = webhookSender;
        }

        /// <inheritdoc />
        public async Task OnEvent(UserUpdatedEventArgs eventArgs)
        {
            var dataObject = DataObjectHelpers
                .GetBaseDataObject(_applicationHost, NotificationType.UserUpdated)
                .AddUserData(eventArgs.Argument);

            await _webhookSender.SendNotification(NotificationType.UserUpdated, dataObject)
                .ConfigureAwait(false);
        }
    }
}