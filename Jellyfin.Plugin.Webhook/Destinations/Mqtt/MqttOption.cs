using System;
using HandlebarsDotNet;
using Jellyfin.Plugin.Webhook.Helpers;
using MQTTnet.Protocol;

namespace Jellyfin.Plugin.Webhook.Destinations.Mqtt;

/// <summary>
/// Mqtt specific option.
/// </summary>
public class MqttOption : BaseOption
{
    private HandlebarsTemplate<object, string>? _compiledTopicTemplate;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttOption"/> class.
    /// </summary>
    public MqttOption()
    {
        Guid = Guid.NewGuid();
    }

    /// <summary>
    /// Gets the Guid for MqttOption.
    /// </summary>
    public Guid Guid { get; }

    /// <summary>
    /// Gets or sets the MqttServer.
    /// </summary>
    public string MqttServer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the mqtt port.
    /// </summary>
    public int MqttPort { get; set; } = 1883;

    /// <summary>
    /// Gets or sets the BaseTopic.
    /// </summary>
    public string Topic { get; set; } = string.Empty;

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
    /// Gets or sets a value indicating whether to use TLS.
    /// </summary>
    public bool UseTls { get; set; }

    /// <summary>
    /// Gets or sets the Quality of service level.
    /// </summary>
    public MqttQualityOfServiceLevel QosLevel { get; set; }

    /// <summary>
    /// Gets the compiled handlebars topic template.
    /// </summary>
    /// <returns>The compiled handlebars subject template.</returns>
    public HandlebarsTemplate<object, string> GetCompiledTopicTemplate() => _compiledTopicTemplate ??= Handlebars.Compile(HandlebarsFunctionHelpers.Base64Decode(Topic));
}
