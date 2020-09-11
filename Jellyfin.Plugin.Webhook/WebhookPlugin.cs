using System;
using System.Collections.Generic;
using Jellyfin.Plugin.Webhook.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.Webhook
{
    /// <summary>
    /// Plugin entrypoint.
    /// </summary>
    public class WebhookPlugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        private readonly Guid _id = new Guid("529397D0-A0AA-43DB-9537-7CFDE936C1E3");

        /// <summary>
        /// Initializes a new instance of the <see cref="WebhookPlugin"/> class.
        /// </summary>
        /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
        /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
        public WebhookPlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        /// <summary>
        /// Gets current plugin instance.
        /// </summary>
        public static WebhookPlugin Instance { get; private set; }

        /// <inheritdoc />
        public override Guid Id => _id;

        /// <inheritdoc />
        public override string Name => "Webhook";

        /// <inheritdoc />
        public override string Description => "Sends notifications to various services via webhooks.";

        /// <inheritdoc />
        public IEnumerable<PluginPageInfo> GetPages()
        {
            yield return new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = GetType().Namespace + ".Configuration.Web.config.html"
            };

            yield return new PluginPageInfo
            {
                Name = $"{Name}JS",
                EmbeddedResourcePath = GetType().Namespace + ".Configuration.Web.config.js"
            };
        }
    }
}