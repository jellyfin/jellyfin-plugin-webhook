using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.Plugin.Webhook.Notifiers.ItemAddedNotifier;

/// <summary>
/// Scheduled task that processes item added events.
/// </summary>
public class ItemAddedScheduledTask : IScheduledTask, IConfigurableScheduledTask
{
    private const int RecheckIntervalSec = 30;
    private readonly IItemAddedManager _itemAddedManager;
    private readonly ILocalizationManager _localizationManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemAddedScheduledTask"/> class.
    /// </summary>
    /// <param name="itemAddedManager">Instance of the <see cref="IItemAddedManager"/> interface.</param>
    /// <param name="localizationManager">Instance of the <see cref="ILocalizationManager"/> interface.</param>
    public ItemAddedScheduledTask(
        IItemAddedManager itemAddedManager,
        ILocalizationManager localizationManager)
    {
        _itemAddedManager = itemAddedManager;
        _localizationManager = localizationManager;
    }

    /// <inheritdoc />
    public string Name => "Webhook Item Added Notifier";

    /// <inheritdoc />
    public string Key => "WebhookItemAdded";

    /// <inheritdoc />
    public string Description => "Processes item added queue";

    /// <inheritdoc />
    public string Category => _localizationManager.GetLocalizedString("TasksLibraryCategory");

    /// <inheritdoc />
    public bool IsHidden => false;

    /// <inheritdoc />
    public bool IsEnabled => true;

    /// <inheritdoc />
    public bool IsLogged => false;

    /// <inheritdoc />
    public Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        return _itemAddedManager.ProcessItemsAsync();
    }

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return new[]
        {
            new TaskTriggerInfo
            {
                Type = TaskTriggerInfo.TriggerInterval,
                IntervalTicks = TimeSpan.FromSeconds(RecheckIntervalSec).Ticks
            }
        };
    }
}
