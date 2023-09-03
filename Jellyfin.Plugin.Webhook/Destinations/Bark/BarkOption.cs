namespace Jellyfin.Plugin.Webhook.Destinations.Bark;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Jellyfin.Extensions.Json;
using Jellyfin.Plugin.Webhook.Destinations.Generic;

/// <summary>
/// Bark specific options.
/// </summary>
public class BarkOption : BaseOption
{
    /// <summary>
    /// Gets or sets the Device key.
    /// </summary>
    [JsonPropertyName("device_key")]
    public string? DeviceKey { get; set; }

    /// <summary>
    /// Gets or sets 推送中断级别
    /// active：默认值，系统会立即亮屏显示通知.
    /// timeSensitive：时效性通知，可在专注状态下显示通知.
    /// passive：仅将通知添加到通知列表，不会亮屏提醒.
    /// </summary>
    [JsonPropertyName("level")]
    public string? Level { get; set; }

    /// <summary>
    /// Gets or sets icon url
    /// 为推送设置自定义图标，设置的图标将替换默认Bark图标。
    /// 图标会自动缓存在本机，相同的图标 URL 仅下载一次.
    /// </summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets Group
    /// 对消息进行分组，推送将按group分组显示在通知中心中。也可在历史消息列表中选择查看不同的群组.
    /// </summary>
    [JsonPropertyName("group")]
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets Group IsArchive
    /// 传 1 保存推送，传其他的不保存推送，不传按APP内设置来决定是否保存.
    /// </summary>
    [JsonPropertyName("isArchive")]
    public int? IsArchive { get; set; }

    /// <summary>
    /// Gets or sets JumpUrl
    /// 点击推送时，跳转的URL ，支持URL Scheme 和 Universal Link.
    /// </summary>
    [JsonPropertyName("url")]
    public string? JumpUrl { get; set; }

    /// <summary>
    /// Gets or sets extra Headers other than application/json.
    /// </summary>
    public GenericOptionValue[] Headers { get; set; } = Array.Empty<GenericOptionValue>();
}
