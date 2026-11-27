namespace MusicPlayer.Behavioral.Observer;

/// <summary>
/// OBSERVER - concrete observer: warns when the volume is raised above a safe level.
/// It only WARNS; it does not lower the volume itself. Changing the player from inside
/// Update would trigger another VolumeChanged event and could start an update loop.
/// </summary>
public sealed class LoudVolumeWarning : IPlaybackObserver
{
    private readonly int _threshold;
    private readonly TextWriter _writer;

    public LoudVolumeWarning(int threshold, TextWriter writer)
    {
        _threshold = threshold;
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    public int WarningCount { get; private set; }

    // OBSERVER: reacts only to VolumeChanged events above the threshold.
    public void Update(PlaybackEvent playbackEvent)
    {
        if (playbackEvent.Kind == PlaybackEventKind.VolumeChanged && playbackEvent.Volume > _threshold)
        {
            WarningCount++;
            _writer.WriteLine($"   [warning] Volume {playbackEvent.Volume}% is above {_threshold}%: protect your hearing!");
        }
    }
}
