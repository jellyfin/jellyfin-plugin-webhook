using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet.Extensions.ManagedClient;

namespace Jellyfin.Plugin.Webhook.Destinations.Mqtt;

/// <summary>
/// Client for the <see cref="MqttOption"/>.
/// </summary>
public class MqttClient : BaseClient, IWebhookClient<MqttOption>
{
    private readonly ILogger<MqttClient> _logger;
    private readonly IMqttClients _mqttClients;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttClient"/> class.
    /// </summary>
    /// <param name="logger">Instance of the <see cref="ILogger{MqttClient}"/> interface.</param>
    /// <param name="mqttClients">Instance of the <see cref="IMqttClients"/> interface.</param>
    public MqttClient(ILogger<MqttClient> logger, IMqttClients mqttClients)
    {
        _logger = logger;
        _mqttClients = mqttClients;
    }

    /// <inheritdoc />
    public async Task SendAsync(MqttOption option, Dictionary<string, object> data)
    {
        try
        {
            if (!SendWebhook(_logger, option, data))
            {
                return;
            }

            var body = option.GetMessageBody(data);
            if (!SendMessageBody(_logger, option, body))
            {
                return;
            }

            var client = _mqttClients.GetClient(option.Guid);
            if (client?.IsConnected != true)
            {
                _logger.LogDebug("MQTT error, not connected {@server}", option.MqttServer);
                return;
            }

            var topic = option.GetCompiledTopicTemplate()(data);

            await client.EnqueueAsync(topic, body).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogDebug(e, "Error sending MQTT notification");
        }
    }
}
