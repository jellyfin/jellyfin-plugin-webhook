using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.Webhook.Helpers
{
    /// <summary>
    /// Periodic async helper.
    /// </summary>
    public static class PeriodicAsyncHelper
    {
        /// <summary>
        /// Runs an async function periodically.
        /// </summary>
        /// <param name="taskFactory">The task factory.</param>
        /// <param name="interval">The run interval.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task PeriodicAsync(
            Func<Task> taskFactory,
            TimeSpan interval,
            CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var delayTask = Task.Delay(interval, cancellationToken);
                await taskFactory();
                await delayTask;
            }
        }
    }
}