using System.Threading.Tasks;
using Jellyfin.Data.Events;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Common;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Session;

namespace Jellyfin.Plugin.Webhook.Notifiers
{
    /// <summary>
    /// Authentication failure notifier.
    /// </summary>
    public class AuthenticationFailureNotifier : IEventConsumer<GenericEventArgs<AuthenticationRequest>>
    {
        private readonly IApplicationHost _applicationHost;
        private readonly IWebhookSender _webhookSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationFailureNotifier"/> class.
        /// </summary>
        /// <param name="applicationHost">Instance of the <see cref="IApplicationHost"/> interface.</param>
        /// <param name="webhookSender">Instance of the <see cref="IWebhookSender"/> interface.</param>
        public AuthenticationFailureNotifier(
            IApplicationHost applicationHost,
            IWebhookSender webhookSender)
        {
            _applicationHost = applicationHost;
            _webhookSender = webhookSender;
        }

        /// <inheritdoc />
        public async Task OnEvent(GenericEventArgs<AuthenticationRequest> eventArgs)
        {
            if (eventArgs.Argument == null)
            {
                return;
            }

            var dataObject = DataObjectHelpers
                .GetBaseDataObject(_applicationHost, NotificationType.AuthenticationFailure);
            dataObject[nameof(eventArgs.Argument.App)] = eventArgs.Argument.App;
            dataObject[nameof(eventArgs.Argument.Username)] = eventArgs.Argument.Username;
            dataObject[nameof(eventArgs.Argument.UserId)] = eventArgs.Argument.UserId;
            dataObject[nameof(eventArgs.Argument.AppVersion)] = eventArgs.Argument.AppVersion;
            dataObject[nameof(eventArgs.Argument.DeviceId)] = eventArgs.Argument.DeviceId;
            dataObject[nameof(eventArgs.Argument.DeviceName)] = eventArgs.Argument.DeviceName;

            await _webhookSender.SendNotification(NotificationType.AuthenticationFailure, dataObject);
        }
    }
}