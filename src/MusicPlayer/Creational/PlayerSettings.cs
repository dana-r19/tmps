namespace MusicPlayer.Creational;

/// <summary>
/// SINGLETON (Lab 1): one shared settings object. The private constructor prevents
/// "new PlayerSettings()", and Lazy&lt;T&gt; makes the creation thread-safe.
/// </summary>
public sealed class PlayerSettings
{
    private static readonly Lazy<PlayerSettings> LazyInstance =
        new Lazy<PlayerSettings>(() => new PlayerSettings());

    private PlayerSettings() { }

    public static PlayerSettings Instance => LazyInstance.Value;

    public int DefaultVolume { get; set; } = 80;
    public int DefaultFadeInSeconds { get; set; } = 3;
    public int DefaultLoopCount { get; set; } = 2;
}
