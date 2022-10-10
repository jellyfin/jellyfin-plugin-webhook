using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Events;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.Plugin.Webhook.Notifiers;

/// <summary>
/// Task completed notifier.
/// </summary>
public class TaskCompletedNotifier : IEventConsumer<TaskCompletionEventArgs>
{
    private readonly IServerApplicationHost _applicationHost;
    private readonly IWebhookSender _webhookSender;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskCompletedNotifier"/> class.
    /// </summary>
    /// <param name="applicationHost">Instance of the <see cref="IServerApplicationHost"/> interface.</param>
    /// <param name="webhookSender">Instance of the <see cref="IWebhookSender"/> interface.</param>
    public TaskCompletedNotifier(
        IServerApplicationHost applicationHost,
        IWebhookSender webhookSender)
    {
        _applicationHost = applicationHost;
        _webhookSender = webhookSender;
    }

    /// <inheritdoc />
    public async Task OnEvent(TaskCompletionEventArgs eventArgs)
    {
        var dataObject = DataObjectHelpers
            .GetBaseDataObject(_applicationHost, NotificationType.TaskCompleted);
        dataObject["TaskId"] = eventArgs.Task.Id;
        dataObject["TaskName"] = eventArgs.Task.Name;
        dataObject["TaskDescription"] = eventArgs.Task.Description;
        dataObject["TaskCategory"] = eventArgs.Task.Category;
        dataObject["TaskState"] = eventArgs.Task.State.ToString();
        dataObject["ResultId"] = eventArgs.Result.Id;
        dataObject["ResultKey"] = eventArgs.Result.Key;
        dataObject["ResultName"] = eventArgs.Result.Name;
        dataObject["ResultStatus"] = eventArgs.Result.Status.ToString();
        dataObject["StartTime"] = eventArgs.Result.StartTimeUtc;
        dataObject["EndTime"] = eventArgs.Result.EndTimeUtc;
        dataObject["ResultErrorMessage"] = eventArgs.Result.ErrorMessage;
        dataObject["ResultLongErrorMessage"] = eventArgs.Result.LongErrorMessage;

        await _webhookSender.SendNotification(NotificationType.TaskCompleted, dataObject)
            .ConfigureAwait(false);
    }
}
