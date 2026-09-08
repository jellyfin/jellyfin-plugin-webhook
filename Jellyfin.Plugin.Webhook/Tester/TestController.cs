using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Extensions.Json;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Tester
{
    /// <summary>
    /// API endpoints to test webhooks.
    /// </summary>
    [ApiController]
    [Route("Webhook")]
    [Authorize]
    public class TestController : ControllerBase
    {
        private readonly IServerApplicationHost _applicationHost;
        private readonly ILogger<WebhookSender> _logger;
        private readonly IWebhookSender _webhookSender;
        private static readonly JsonSerializerOptions _jsonIndentedOptions = new JsonSerializerOptions(JsonDefaults.Options)
        {
            WriteIndented = true
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="TestController"/> class.
        /// </summary>
        /// <param name="applicationHost">The application host.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="webhookSender">The webhook sender used to send test notifications.</param>
        public TestController(IServerApplicationHost applicationHost, ILogger<WebhookSender> logger, IWebhookSender webhookSender)
        {
            _applicationHost = applicationHost;
            _logger = logger;
            _webhookSender = webhookSender;
        }

        /// <summary>
        /// API endpoint to test webhooks.
        /// </summary>
        /// <param name="testWebhooksDto">Contains the data for testing the webhooks.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpPost("Test")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> TestWebhooks([FromBody] TestWebhooksDto testWebhooksDto)
        {
            NotificationType notificationType = testWebhooksDto.NotificationType;
            string? itemTypeString = testWebhooksDto.ItemType;
            string? testData64 = testWebhooksDto.TestData;

            // Get ItemType from string
            Type? itemType;
            switch (itemTypeString)
            {
                case "Movie":
                    itemType = typeof(Movie);
                    break;
                case "Episode":
                    itemType = typeof(Episode);
                    break;
                case "Season":
                    itemType = typeof(Season);
                    break;
                case "Series":
                    itemType = typeof(Series);
                    break;
                case "Album":
                    itemType = typeof(MusicAlbum);
                    break;
                case "Song":
                    itemType = typeof(Audio);
                    break;
                case "Video":
                    itemType = typeof(Video);
                    break;
                default:
                    itemType = null;
                    break;
            }

            // Create base data object with notification type and item type
            var dataObject = DataObjectHelpers.GetBaseDataObject(_applicationHost, notificationType);
            dataObject["ItemType"] = itemType?.Name ?? "None";

            // If test data is provided, decode it from Base64 and merge it into the data object. It will override the base data if there are any key conflicts.
            if (!string.IsNullOrEmpty(testData64))
            {
                try
                {
                    string testDataJson = HandlebarsFunctionHelpers.Base64Decode(testData64);
                    var testData = JsonSerializer.Deserialize<Dictionary<string, object>>(testDataJson) ?? [];
                    foreach (var (key, value) in testData)
                    {
                        dataObject[key] = value;
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning("Invalid test data: {Message}", ex.Message);
                    return BadRequest($"Invalid test data: {ex.Message}");
                }
            }

            // Log the final data object that will be sent with the webhook for debugging
            string prettyJson = JsonSerializer.Serialize(dataObject, _jsonIndentedOptions);
            _logger.LogDebug("Testing webhooks with test data: \n{PrettyJson}", prettyJson);

            // Send the notification
            await _webhookSender.SendNotification(notificationType, dataObject, itemType).ConfigureAwait(false);

            return Ok();
        }
    }
}
