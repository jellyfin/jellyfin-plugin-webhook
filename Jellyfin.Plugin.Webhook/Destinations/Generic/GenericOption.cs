using System.Collections.Generic;

namespace Jellyfin.Plugin.Webhook.Destinations.Generic
{
    /// <summary>
    /// Generic webhook options.
    /// </summary>
    public class GenericOption : BaseOption
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericOption"/> class.
        /// </summary>
        public GenericOption()
        {
            Headers = new Dictionary<string, string>();
            Fields = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Gets or sets the fields.
        /// </summary>
        public Dictionary<string, string> Fields { get; set; }
    }
}