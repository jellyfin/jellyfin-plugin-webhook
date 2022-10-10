using System;

namespace Jellyfin.Plugin.Webhook.Destinations.Generic;

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
        Headers = Array.Empty<GenericOptionValue>();
        Fields = Array.Empty<GenericOptionValue>();
    }

    /// <summary>
    /// Gets or sets the headers.
    /// </summary>
    public GenericOptionValue[] Headers { get; set; }

    /// <summary>
    /// Gets or sets the fields.
    /// </summary>
    public GenericOptionValue[] Fields { get; set; }
}
