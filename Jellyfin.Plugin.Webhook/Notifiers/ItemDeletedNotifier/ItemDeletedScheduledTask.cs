using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.Plugin.Webhook.Notifiers.ItemDeletedNotifier;

/// <summary>
/// Scheduled task that processes item deleted events.
/// </summary>
public class ItemDeletedScheduledTask : IScheduledTask, IConfigurableScheduledTask
{
    private const int RecheckIntervalSec = 30;
    private readonly IItemDeletedManager _itemDeletedManager;
    private readonly ILocalizationManager _localizationManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemDeletedScheduledTask"/> class.
    /// </summary>
    /// <param name="itemDeletedManager">Instance of the <see cref="IItemDeletedManager"/> interface.</param>
    /// <param name="localizationManager">Instance of the <see cref="ILocalizationManager"/> interface.</param>
    public ItemDeletedScheduledTask(
        IItemDeletedManager itemDeletedManager,
        ILocalizationManager localizationManager)
    {
        _itemDeletedManager = itemDeletedManager;
        _localizationManager = localizationManager;
    }

    /// <inheritdoc />
    public string Name => "Webhook Item Deleted Notifier";

    /// <inheritdoc />
    public string Key => "WebhookItemDeleted";

    /// <inheritdoc />
    public string Description => "Processes item deleted queue";

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
        return _itemDeletedManager.ProcessItemsAsync();
    }

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return new[]
        {
                new TaskTriggerInfo
                {
                    Type = TaskTriggerInfoType.IntervalTrigger,
                    IntervalTicks = TimeSpan.FromSeconds(RecheckIntervalSec).Ticks
                }
        };
    }
}
