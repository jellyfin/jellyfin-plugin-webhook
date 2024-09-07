using System;

namespace Jellyfin.Plugin.Webhook.Models;

/// <summary>
/// Queued item container.
/// </summary>
public class QueuedItemContainer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueuedItemContainer"/> class.
    /// </summary>
    /// <param name="id">The item id.</param>
    /// <param name="name">The item name (nullable).</param>
    /// <param name="itemType">The item type.</param>
    public QueuedItemContainer(Guid id, string? name = null, string? itemType = null)
    {
        ItemId = id;
        Name = name;
        ItemType = itemType;
        RetryCount = 0;
    }

    /// <summary>
    /// Gets or sets the item name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the item type.
    /// </summary>
    public string? ItemType { get; set; }

    /// <summary>
    /// Gets or sets the current item id.
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Gets or sets the current retry count.
    /// </summary>
    public int RetryCount { get; set; }
}
