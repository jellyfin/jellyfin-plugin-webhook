using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Hosting;

namespace Jellyfin.Plugin.Webhook.Notifiers.ItemDeletedNotifier;

/// <summary>
/// Notifier when a library item is deleted.
/// </summary>
public class ItemDeletedNotifierEntryPoint : IHostedService
{
    private readonly IItemDeletedManager _itemDeletedManager;
    private readonly ILibraryManager _libraryManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemDeletedNotifierEntryPoint"/> class.
    /// </summary>
    /// <param name="itemDeletedManager">Instance of the <see cref="IItemDeletedManager"/> interface.</param>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    public ItemDeletedNotifierEntryPoint(
        IItemDeletedManager itemDeletedManager,
        ILibraryManager libraryManager)
    {
        _itemDeletedManager = itemDeletedManager;
        _libraryManager = libraryManager;
    }

    private void ItemDeletedHandler(object? sender, ItemChangeEventArgs itemChangeEventArgs)
    {
        // Never notify on virtual items.
        if (itemChangeEventArgs.Item.IsVirtualItem)
        {
            return;
        }

        _itemDeletedManager.AddItem(itemChangeEventArgs.Item);
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _libraryManager.ItemRemoved += ItemDeletedHandler;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _libraryManager.ItemRemoved -= ItemDeletedHandler;
        return Task.CompletedTask;
    }
}
