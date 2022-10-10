using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;

namespace Jellyfin.Plugin.Webhook.Notifiers.ItemAddedNotifier;

/// <summary>
/// Item added manager interface.
/// </summary>
public interface IItemAddedManager
{
    /// <summary>
    /// Process the current queue.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task ProcessItemsAsync();

    /// <summary>
    /// Add item to process queue.
    /// </summary>
    /// <param name="item">The added item.</param>
    public void AddItem(BaseItem item);
}
