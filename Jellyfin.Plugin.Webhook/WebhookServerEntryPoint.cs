using System;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Configuration;
using Jellyfin.Plugin.Webhook.Destinations.Mqtt;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Webhook;

/// <summary>
/// WebhookServerEntryPoint.
/// </summary>
public class WebhookServerEntryPoint : IServerEntryPoint
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
        await RunAsync().ConfigureAwait(false);
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
    /// <param name="disposeAll">Dispose of objects.</param>
    protected virtual void Dispose(bool disposeAll)
    {
        WebhookPlugin.Instance!.ConfigurationChanged -= ConfigurationChanged;
    }

    /// <summary>
    /// Run this.
    /// </summary>
    /// <returns>Task.</returns>
    public async Task RunAsync()
    {
        await _mqttClients.UpdateClients(Configuration.MqttOptions).ConfigureAwait(false);
    }
}