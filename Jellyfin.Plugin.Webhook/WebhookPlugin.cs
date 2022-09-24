using System;
using System.Collections.Generic;
using Jellyfin.Plugin.Webhook.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook
{
    /// <summary>
    /// Plugin entrypoint.
    /// </summary>
    public class WebhookPlugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        private readonly Guid _id = new ("71552A5A-5C5C-4350-A2AE-EBE451A30173");
        private readonly ILogger<WebhookPlugin> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebhookPlugin"/> class.
        /// </summary>
        /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
        /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
        /// <param name="logger">Instance of the <see cref="ILogger{WebhookPlugin}"/> interface.</param>
        public WebhookPlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ILogger<WebhookPlugin> logger)
            : base(applicationPaths, xmlSerializer)
        {
            _logger = logger;
            Instance = this;
        }

        /// <summary>
        /// Gets current plugin instance.
        /// </summary>
        public static WebhookPlugin? Instance { get; private set; }

        /// <inheritdoc />
        public override Guid Id => _id;

        /// <inheritdoc />
        public override string Name => "Webhook";

        /// <inheritdoc />
        public override string Description => "Sends notifications to various services via webhooks.";

        /// <inheritdoc />
        public IEnumerable<PluginPageInfo> GetPages()
        {
            var prefix = GetType().Namespace;
            yield return new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = prefix + ".Configuration.Web.config.html"
            };

            yield return new PluginPageInfo
            {
                Name = $"{Name}.js",
                EmbeddedResourcePath = prefix + ".Configuration.Web.config.js"
            };
        }
    }
}
