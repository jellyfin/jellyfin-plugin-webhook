using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Notifiers.UserDataSavedNotifier;

/// <summary>
/// User data saved notifier.
/// </summary>
public class UserDataSavedNotifierEntryPoint : IHostedService
{
    private readonly IServerApplicationHost _applicationHost;
    private readonly IUserDataManager _userDataManager;
    private readonly IUserManager _userManager;
    private readonly ILogger<UserDataSavedNotifierEntryPoint> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserDataSavedNotifierEntryPoint"/> class.
    /// </summary>
    /// <param name="userDataManager">Instance of the <see cref="IUserDataManager"/> interface.</param>
    /// <param name="applicationHost">Instance of the <see cref="IServerApplicationHost"/> interface.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{UserDataChangedNotifierEntryPoint}"/> interface.</param>
    /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
    public UserDataSavedNotifierEntryPoint(
        IServerApplicationHost applicationHost,
        IUserDataManager userDataManager,
        ILogger<UserDataSavedNotifierEntryPoint> logger,
        IUserManager userManager)
    {
        _userDataManager = userDataManager;
        _logger = logger;
        _userManager = userManager;
        _applicationHost = applicationHost;
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

            var specificType = GetSpecificNotificationType(eventArgs.SaveReason, eventArgs.UserData.Played);
            if (specificType is not null)
            {
                await SendNotificationAsync(specificType.Value, eventArgs, user.Username, user.Id).ConfigureAwait(false);
            }

            await SendNotificationAsync(NotificationType.UserDataSaved, eventArgs, user.Username, user.Id).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to send notification");
        }
    }

    private async Task SendNotificationAsync(NotificationType notificationType, UserDataSaveEventArgs eventArgs, string username, Guid userId)
    {
        var dataObject = DataObjectHelpers
            .GetBaseDataObject(_applicationHost, notificationType)
            .AddBaseItemData(eventArgs.Item)
            .AddUserItemData(eventArgs.UserData);

        dataObject["SaveReason"] = eventArgs.SaveReason.ToString();
        dataObject["NotificationUsername"] = username;
        dataObject["UserId"] = userId;

        var scope = _applicationHost.ServiceProvider!.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            var webhookSender = scope.ServiceProvider.GetRequiredService<IWebhookSender>();
            await webhookSender
                .SendNotification(notificationType, dataObject, eventArgs.Item.GetType())
                .ConfigureAwait(false);
        }
    }

    private static NotificationType? GetSpecificNotificationType(UserDataSaveReason saveReason, bool played)
    {
        return saveReason switch
        {
            UserDataSaveReason.TogglePlayed => played ? NotificationType.ItemPlayed : NotificationType.ItemUnplayed,
            _ => null
        };
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _userDataManager.UserDataSaved += UserDataSavedHandler;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _userDataManager.UserDataSaved -= UserDataSavedHandler;
        return Task.CompletedTask;
    }
}
