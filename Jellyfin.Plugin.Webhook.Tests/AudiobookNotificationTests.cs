using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;
using Jellyfin.Plugin.Webhook.Destinations.Discord;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Xunit;

namespace Jellyfin.Plugin.Webhook.Tests;

public class AudiobookNotificationTests
{
    [Fact]
    public void LegacyXmlConfigurationKeepsAudiobooksDisabled()
    {
        var serializer = new XmlSerializer(typeof(DiscordOption));
        using var reader = new StringReader("<DiscordOption><EnableSongs>true</EnableSongs></DiscordOption>");
        var option = Assert.IsType<DiscordOption>(serializer.Deserialize(reader));
        Assert.True(option.EnableSongs);
        Assert.False(option.EnableAudiobooks);
    }

    [Fact]
    public void AudiobookOptInSurvivesXmlConfigurationRoundTrip()
    {
        var serializer = new XmlSerializer(typeof(DiscordOption));
        using var writer = new StringWriter();
        serializer.Serialize(writer, new DiscordOption { EnableAudiobooks = true });
        using var reader = new StringReader(writer.ToString());
        var option = Assert.IsType<DiscordOption>(serializer.Deserialize(reader));
        Assert.True(option.EnableAudiobooks);
        Assert.False(option.EnableSongs);
    }

    [Fact]
    public void ExistingOptionsDoNotOptIntoAudiobooks()
    {
        var option = new DiscordOption { EnableSongs = true };
        Assert.True(WebhookSender.NotifyOnItem(option, typeof(Audio)));
        Assert.False(WebhookSender.NotifyOnItem(option, typeof(AudioBook)));
    }

    [Fact]
    public void AudiobooksCanBeEnabledWithoutEnablingSongs()
    {
        var option = new DiscordOption { EnableAudiobooks = true };
        Assert.True(WebhookSender.NotifyOnItem(option, typeof(AudioBook)));
        Assert.False(WebhookSender.NotifyOnItem(option, typeof(Audio)));
        Assert.True(WebhookSender.NotifyOnItem(option, null));
    }

    [Theory]
    [InlineData("AudioBookPlaybackStart.handlebars", "PlaybackStart", false)]
    [InlineData("AudioBookPlaybackStop.handlebars", "PlaybackStop", false)]
    [InlineData("AudioBookPlaybackStop.handlebars", "PlaybackStop", true)]
    public void AudiobookTemplatesRenderPlaybackPositionAndRuntime(string file, string notificationType, bool completed)
    {
        const string name = "Example \"Book\" & More";
        var item = new TestAudioBook { Name = name, RunTimeTicks = TimeSpan.FromMinutes(630).Ticks };
        var data = new Dictionary<string, object>().AddBaseItemData(item);
        // The test double bypasses server-only media lookups; real events use AudioBook.
        data["ItemType"] = nameof(AudioBook);
        data.AddPlaybackProgressData(new PlaybackProgressEventArgs { PlaybackPositionTicks = TimeSpan.FromMinutes(135).Ticks });
        data["NotificationType"] = notificationType;
        const string listener = "Listener \"One\"\nSecond line";
        data["NotificationUsername"] = listener;
        data["PlayedToCompletion"] = completed;
        HandlebarsFunctionHelpers.RegisterHelpers();
        var template = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Templates", file));
        var option = new DiscordOption { Template = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(template)) };
        using var document = JsonDocument.Parse(option.GetMessageBody(data));
        var embed = document.RootElement.GetProperty("embeds")[0];
        Assert.Contains(name, embed.GetProperty("title").GetString());
        Assert.Contains("02:15:00 / 10:30:00", embed.GetProperty("description").GetString());
        Assert.Contains(listener, embed.GetProperty("description").GetString());
        if (notificationType == "PlaybackStop")
        {
            Assert.Contains(completed ? "completed" : "stopped", embed.GetProperty("title").GetString());
        }
    }

    [Theory]
    [InlineData("AudioBookPlaybackStart.handlebars")]
    [InlineData("AudioBookPlaybackStop.handlebars")]
    public void AudiobookTemplatesDoNotRenderForMusic(string file)
    {
        HandlebarsFunctionHelpers.RegisterHelpers();
        var template = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Templates", file));
        var option = new DiscordOption { Template = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(template)) };
        Assert.Equal(string.Empty, option.GetMessageBody(new Dictionary<string, object> { ["ItemType"] = "Audio" }).Trim());
    }

    private sealed class TestAudioBook : AudioBook
    {
        public override IReadOnlyList<MediaStream> GetMediaStreams() => Array.Empty<MediaStream>();
    }
}
