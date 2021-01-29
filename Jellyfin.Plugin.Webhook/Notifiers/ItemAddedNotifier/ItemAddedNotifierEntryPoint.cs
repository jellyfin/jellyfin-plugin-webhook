using System;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;

namespace Jellyfin.Plugin.Webhook.Notifiers.ItemAddedNotifier
{
    /// <summary>
    /// Notifier when a library item is added.
    /// </summary>
    public class ItemAddedNotifierEntryPoint : IServerEntryPoint
    {
        private readonly IItemAddedManager _itemAddedManager;
        private readonly ILibraryManager _libraryManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemAddedNotifierEntryPoint"/> class.
        /// </summary>
        /// <param name="itemAddedManager">Instance of the <see cref="IItemAddedManager"/> interface.</param>
        /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
        public ItemAddedNotifierEntryPoint(
            IItemAddedManager itemAddedManager,
            ILibraryManager libraryManager)
        {
            _itemAddedManager = itemAddedManager;
            _libraryManager = libraryManager;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public Task RunAsync()
        {
            _libraryManager.ItemAdded += ItemAddedHandler;
            HandlebarsFunctionHelpers.RegisterHelpers();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Dispose all assets.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _libraryManager.ItemAdded -= ItemAddedHandler;
            }
        }

        private void ItemAddedHandler(object? sender, ItemChangeEventArgs itemChangeEventArgs)
        {
            // Never notify on virtual items.
            if (itemChangeEventArgs.Item.IsVirtualItem)
            {
                return;
            }

            _itemAddedManager.AddItem(itemChangeEventArgs.Item);
        }
    }
}