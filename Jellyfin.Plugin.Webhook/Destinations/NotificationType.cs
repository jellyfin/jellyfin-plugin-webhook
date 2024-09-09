namespace Jellyfin.Plugin.Webhook.Destinations;

/// <summary>
/// The type of notification.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// No notification type.
    /// </summary>
    None = 0,

    /// <summary>
    /// Item added notification.
    /// </summary>
    ItemAdded = 1,

    /// <summary>
    /// Generic notification.
    /// </summary>
    Generic = 2,

    /// <summary>
    /// Playback start notification.
    /// </summary>
    PlaybackStart = 3,

    /// <summary>
    /// Playback progress notification.
    /// </summary>
    PlaybackProgress = 4,

    /// <summary>
    /// Playback stop notification.
    /// </summary>
    PlaybackStop = 5,

    /// <summary>
    /// Subtitle download failure.
    /// </summary>
    SubtitleDownloadFailure = 6,

    /// <summary>
    /// Authentication failure.
    /// </summary>
    AuthenticationFailure = 7,

    /// <summary>
    /// Authentication success.
    /// </summary>
    AuthenticationSuccess = 8,

    /// <summary>
    /// Session started.
    /// </summary>
    SessionStart = 9,

    /// <summary>
    /// Server pending restart.
    /// </summary>
    PendingRestart = 10,

    /// <summary>
    /// Task completed.
    /// </summary>
    TaskCompleted = 11,

    /// <summary>
    /// Plugin installation cancelled.
    /// </summary>
    PluginInstallationCancelled = 12,

    /// <summary>
    /// Plugin installation failed.
    /// </summary>
    PluginInstallationFailed = 13,

    /// <summary>
    /// Plugin installed.
    /// </summary>
    PluginInstalled = 14,

    /// <summary>
    /// Plugin installing.
    /// </summary>
    PluginInstalling = 15,

    /// <summary>
    /// Plugin uninstalled.
    /// </summary>
    PluginUninstalled = 16,

    /// <summary>
    /// Plugin updated.
    /// </summary>
    PluginUpdated = 17,

    /// <summary>
    /// User created.
    /// </summary>
    UserCreated = 18,

    /// <summary>
    /// User deleted.
    /// </summary>
    UserDeleted = 19,

    /// <summary>
    /// User locked out.
    /// </summary>
    UserLockedOut = 20,

    /// <summary>
    /// User password changed.
    /// </summary>
    UserPasswordChanged = 21,

    /// <summary>
    /// User updated.
    /// </summary>
    UserUpdated = 22,

    /// <summary>
    /// User data saved.
    /// </summary>
    UserDataSaved = 23,

    /// <summary>
    /// Item Deleted notification.
    /// </summary>
    ItemDeleted = 24
}
