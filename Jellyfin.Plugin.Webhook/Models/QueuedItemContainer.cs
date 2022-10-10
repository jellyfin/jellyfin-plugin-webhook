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
    public QueuedItemContainer(Guid id)
    {
        ItemId = id;
        RetryCount = 0;
    }

    /// <summary>
    /// Gets or sets the current retry count.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the current item id.
    /// </summary>
    public Guid ItemId { get; set; }
}
