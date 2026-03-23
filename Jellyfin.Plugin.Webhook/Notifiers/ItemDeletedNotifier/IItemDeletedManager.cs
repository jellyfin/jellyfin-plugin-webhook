using MediaBrowser.Controller.Entities;

namespace Jellyfin.Plugin.Webhook.Notifiers.ItemDeletedNotifier;

/// <summary>
/// Item deleted manager interface.
/// </summary>
public interface IItemDeletedManager
{
    /// <summary>
    /// Add item and fire the webhook after a short delay.
    /// </summary>
    /// <param name="item">The deleted item.</param>
    public void AddItem(BaseItem item);
}
