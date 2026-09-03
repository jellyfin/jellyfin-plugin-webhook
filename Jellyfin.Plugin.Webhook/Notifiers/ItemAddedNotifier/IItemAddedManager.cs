using MediaBrowser.Controller.Entities;

namespace Jellyfin.Plugin.Webhook.Notifiers.ItemAddedNotifier;

/// <summary>
/// Item added manager interface.
/// </summary>
public interface IItemAddedManager
{
    /// <summary>
    /// Add item to the pending queue. The webhook fires when
    /// <see cref="HandleItemUpdated"/> sees this item with metadata.
    /// </summary>
    /// <param name="item">The added item.</param>
    public void AddItem(BaseItem item);

    /// <summary>
    /// Called when any library item is updated. If the item is in the
    /// pending queue and now has provider IDs, fires the webhook.
    /// </summary>
    /// <param name="item">The updated item.</param>
    public void HandleItemUpdated(BaseItem item);
}
