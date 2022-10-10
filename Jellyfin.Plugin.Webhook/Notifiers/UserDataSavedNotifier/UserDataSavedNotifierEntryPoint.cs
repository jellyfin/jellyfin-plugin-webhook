using System;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Notifiers.UserDataSavedNotifier;

/// <summary>
/// User data saved notifier.
/// </summary>
public class UserDataSavedNotifierEntryPoint : IServerEntryPoint
{
    private readonly IWebhookSender _webhookSender;
    private readonly IServerApplicationHost _applicationHost;
    private readonly IUserDataManager _userDataManager;
    private readonly IUserManager _userManager;
    private readonly ILogger<UserDataSavedNotifierEntryPoint> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserDataSavedNotifierEntryPoint"/> class.
    /// </summary>
    /// <param name="webhookSender">Instance of the <see cref="IWebhookSender"/> interface.</param>
    /// <param name="userDataManager">Instance of the <see cref="IUserDataManager"/> interface.</param>
    /// <param name="applicationHost">Instance of the <see cref="IServerApplicationHost"/> interface.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{UserDataChangedNotifierEntryPoint}"/> interface.</param>
    /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
    public UserDataSavedNotifierEntryPoint(
        IWebhookSender webhookSender,
        IServerApplicationHost applicationHost,
        IUserDataManager userDataManager,
        ILogger<UserDataSavedNotifierEntryPoint> logger,
        IUserManager userManager)
    {
        _userDataManager = userDataManager;
        _logger = logger;
        _userManager = userManager;
        _applicationHost = applicationHost;
        _webhookSender = webhookSender;
    }

    /// <inheritdoc />
    public Task RunAsync()
    {
        _userDataManager.UserDataSaved += UserDataSavedHandler;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose.
    /// </summary>
    /// <param name="disposing">Dispose all assets.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _userDataManager.UserDataSaved -= UserDataSavedHandler;
        }
    }

    private async void UserDataSavedHandler(object? sender, UserDataSaveEventArgs eventArgs)
    {
        try
        {
            if (eventArgs.Item is null)
            {
                return;
            }

            if (eventArgs.Item.IsThemeMedia)
            {
                // Don't report theme song or local trailer playback.
                return;
            }

            var user = _userManager.GetUserById(eventArgs.UserId);
            if (user is null)
            {
                return;
            }

            var dataObject = DataObjectHelpers
                .GetBaseDataObject(_applicationHost, NotificationType.UserDataSaved)
                .AddBaseItemData(eventArgs.Item)
                .AddUserItemData(eventArgs.UserData);

            dataObject["SaveReason"] = eventArgs.SaveReason.ToString();
            dataObject["NotificationUsername"] = user.Username;
            dataObject["UserId"] = user.Id;

            await _webhookSender.SendNotification(NotificationType.UserDataSaved, dataObject, eventArgs.Item.GetType())
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to send notification");
        }
    }
}
