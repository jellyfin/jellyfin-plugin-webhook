using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Extensions;

/// <summary>
/// Extension methods for <see cref="HttpResponseMessage"/>.
/// </summary>
public static class HttpResponseMessageExtensions
{
    /// <summary>
    /// Log a warning message if the <paramref name="response"/> contains an error status code.
    /// </summary>
    /// <param name="response">The HTTP response to log if failed.</param>
    /// <param name="logger">The logger to use to log the warning.</param>
    /// <returns>A task representing the async operation.</returns>
    public static async Task LogIfFailedAsync(this HttpResponseMessage response, ILogger logger)
    {
        // Don't log anything for successful responses
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        // Log the request that caused the failed response, if available
        var request = response.RequestMessage;
        if (request is not null)
        {
            var requestStr = request.Content is not null
                ? await request.Content.ReadAsStringAsync().ConfigureAwait(false)
                : "<empty request body>";
            logger.LogWarning("Notification failed with {Method} request to {Url}: {Content}", request.Method, request.RequestUri, requestStr);
        }

        // Log the response
        var responseStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        logger.LogWarning("Notification failed with response status code {StatusCode}: {Content}", response.StatusCode, responseStr);
    }
}
