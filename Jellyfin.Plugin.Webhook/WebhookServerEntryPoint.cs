using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Configuration;
using Jellyfin.Plugin.Webhook.Destinations.Mqtt;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Hosting;

namespace Jellyfin.Plugin.Webhook;

/// <summary>
/// WebhookServerEntryPoint.
/// </summary>
public class WebhookServerEntryPoint : IHostedService
{
    private readonly IMqttClients _mqttClients;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookServerEntryPoint"/> class.
    /// </summary>
    /// <param name="mqttClients">Instance of the <see cref="IMqttClients"/> interface.</param>
    public WebhookServerEntryPoint(IMqttClients mqttClients)
    {
        _mqttClients = mqttClients;
        WebhookPlugin.Instance!.ConfigurationChanged += ConfigurationChanged;
    }

    private static PluginConfiguration Configuration =>
        WebhookPlugin.Instance!.Configuration;

    private async void ConfigurationChanged(object? sender, BasePluginConfiguration e)
    {
        await _mqttClients.UpdateClients(Configuration.MqttOptions).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _mqttClients.UpdateClients(Configuration.MqttOptions).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        WebhookPlugin.Instance!.ConfigurationChanged -= ConfigurationChanged;
        return Task.CompletedTask;
    }
}
