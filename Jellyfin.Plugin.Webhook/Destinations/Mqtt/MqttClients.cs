using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace Jellyfin.Plugin.Webhook.Destinations.Mqtt;

/// <summary>
/// MqttClients.
/// </summary>
public class MqttClients : IMqttClients, IDisposable
{
    private readonly ILogger<MqttClients> _logger;
    private readonly Dictionary<Guid, IManagedMqttClient> _managedMqttClients = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttClients"/> class.
    /// </summary>
    /// <param name="logger">Instance of the <see cref="ILogger{MqttClients}"/> interface.</param>
    public MqttClients(ILogger<MqttClients> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task UpdateClients(MqttOption[] options)
    {
        try
        {
            foreach (var client in _managedMqttClients.Values.Where(c => c.IsConnected))
            {
                await client.StopAsync().ConfigureAwait(false);
                client.ConnectingFailedAsync -= Client_ConnectingFailedAsync;
                client.ConnectedAsync -= Client_ConnectedAsync;
                client.DisconnectedAsync -= Client_DisconnectedAsync;
            }

            _managedMqttClients.Clear();

            foreach (var option in options)
            {
                var messageBuilder = new MqttClientOptionsBuilder()
                    .WithTcpServer(option.MqttServer, option.MqttPort)
                    .WithWillQualityOfServiceLevel(option.QosLevel)
                    .WithClientId(option.Guid.ToString());

                if (option.UseCredentials && !string.IsNullOrEmpty(option.Username) && !string.IsNullOrEmpty(option.Password))
                {
                    messageBuilder.WithCredentials(option.Username, option.Password);
                }

                if (option.UseTls)
                {
                    messageBuilder.WithTlsOptions(c => c.UseTls());
                }

                var clientOptions = messageBuilder.Build();

                var managedOptions = new ManagedMqttClientOptionsBuilder()
                    .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                    .WithClientOptions(clientOptions)
                    .Build();

                var client = new MqttFactory().CreateManagedMqttClient();
                client.ConnectingFailedAsync += Client_ConnectingFailedAsync;
                client.ConnectedAsync += Client_ConnectedAsync;
                client.DisconnectedAsync += Client_DisconnectedAsync;
                _managedMqttClients.Add(option.Guid, client);

                await client.StartAsync(managedOptions).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error adding/starting MQTT Clients");
        }
    }

    private Task Client_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
    {
        var message = arg.Reason;
        _logger.LogDebug(arg.Exception, "MQTT Client disconnected. Exception: {@message}", message);

        return Task.CompletedTask;
    }

    private Task Client_ConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        _logger.LogDebug("MQTT Client connected.");
        return Task.CompletedTask;
    }

    private Task Client_ConnectingFailedAsync(ConnectingFailedEventArgs arg)
    {
        _logger.LogDebug(arg.Exception, "MQTT Client connection failed.");
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public IManagedMqttClient? GetClient(Guid guid)
    {
        if (_managedMqttClients.TryGetValue(guid, out var client))
        {
            return client;
        }

        return null;
    }

    /// <summary>
    /// Dispose Objects.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose.
    /// </summary>
    /// <param name="disposeAll">somthing.</param>
    protected virtual void Dispose(bool disposeAll)
    {
        foreach (var client in _managedMqttClients.Values)
        {
            client.Dispose();
        }
    }
}
