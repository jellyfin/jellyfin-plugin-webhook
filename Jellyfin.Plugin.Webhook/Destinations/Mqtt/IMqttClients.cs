using System;
using System.Threading.Tasks;
using MQTTnet.Extensions.ManagedClient;

namespace Jellyfin.Plugin.Webhook.Destinations.Mqtt;

/// <summary>
/// IMqttClients.
/// </summary>
public interface IMqttClients
{
    /// <summary>
    /// Update Clients with options.
    /// </summary>
    /// <param name="options">Instance of the <see cref="MqttOption"/>.</param>
    /// <returns>Task.</returns>
    Task UpdateClients(MqttOption[] options);

    /// <summary>
    /// Get Managed Mqtt Clients.
    /// </summary>
    /// <param name="guid">guid of MqttOption.</param>
    /// <returns>Instance of the <see cref="IManagedMqttClient"/> interface.</returns>
    IManagedMqttClient? GetClient(Guid guid);
}
