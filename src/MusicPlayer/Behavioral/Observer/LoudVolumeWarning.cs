namespace MusicPlayer.Behavioral.Observer;

/// <summary>OBSERVER - concrete observer: warns when the volume is raised above a safe level.</summary>
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

    public void Update(PlaybackEvent playbackEvent)
    {
        if (playbackEvent.Kind == PlaybackEventKind.VolumeChanged && playbackEvent.Volume > _threshold)
        {
            WarningCount++;
            _writer.WriteLine($"   [warning] Volume {playbackEvent.Volume}% is above {_threshold}%: protect your hearing!");
        }
    }
}
