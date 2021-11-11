using System.Threading.Tasks;
using Jellyfin.Data.Events;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Authentication;
using MediaBrowser.Controller.Events;

namespace Jellyfin.Plugin.Webhook.Notifiers
{
    /// <summary>
    /// Authentication success notifier.
    /// </summary>
    public class AuthenticationSuccessNotifier : IEventConsumer<GenericEventArgs<AuthenticationResult>>
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
        public async Task OnEvent(GenericEventArgs<AuthenticationResult> eventArgs)
        {
            if (eventArgs.Argument is null)
            {
                return;
            }

            var dataObject = DataObjectHelpers
                .GetBaseDataObject(_applicationHost, NotificationType.AuthenticationSuccess)
                .AddUserData(eventArgs.Argument.User);

            await _webhookSender.SendNotification(NotificationType.AuthenticationSuccess, dataObject)
                .ConfigureAwait(false);
        }
    }
}
