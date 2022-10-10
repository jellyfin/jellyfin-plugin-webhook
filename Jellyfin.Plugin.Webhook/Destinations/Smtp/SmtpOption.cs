using HandlebarsDotNet;
using Jellyfin.Plugin.Webhook.Helpers;

namespace Jellyfin.Plugin.Webhook.Destinations.Smtp;

/// <summary>
/// Smtp specific option.
/// </summary>
public class SmtpOption : BaseOption
{
    private HandlebarsTemplate<object, string>? _compiledSubjectTemplate;

    /// <summary>
    /// Gets or sets the sender address.
    /// </summary>
    public string SenderAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the receiver address.
    /// </summary>
    public string ReceiverAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the smtp server.
    /// </summary>
    public string SmtpServer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the smtp port.
    /// </summary>
    public int SmtpPort { get; set; } = 25;

    /// <summary>
    /// Gets or sets a value indicating whether use credentials.
    /// </summary>
    public bool UseCredentials { get; set; }

    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to use ssl.
    /// </summary>
    public bool UseSsl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the body is html.
    /// </summary>
    public bool IsHtml { get; set; }

    /// <summary>
    /// Gets or sets the email subject template.
    /// </summary>
    public string SubjectTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Gets the compiled handlebars subject template.
    /// </summary>
    /// <returns>The compiled handlebars subject template.</returns>
    public HandlebarsTemplate<object, string> GetCompiledSubjectTemplate()
    {
        return _compiledSubjectTemplate ??= Handlebars.Compile(HandlebarsFunctionHelpers.Base64Decode(SubjectTemplate));
    }
}
