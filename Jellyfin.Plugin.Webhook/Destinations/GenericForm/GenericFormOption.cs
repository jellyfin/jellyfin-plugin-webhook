using System;

namespace Jellyfin.Plugin.Webhook.Destinations.GenericForm;

/// <summary>
/// Generic form webhook options.
/// </summary>
public class GenericFormOption : BaseOption
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenericFormOption"/> class.
    /// </summary>
    public GenericFormOption()
    {
        Headers = Array.Empty<GenericFormOptionValue>();
        Fields = Array.Empty<GenericFormOptionValue>();
    }

    /// <summary>
    /// Gets or sets the headers.
    /// </summary>
    public GenericFormOptionValue[] Headers { get; set; }

    /// <summary>
    /// Gets or sets the fields.
    /// </summary>
    public GenericFormOptionValue[] Fields { get; set; }
}
