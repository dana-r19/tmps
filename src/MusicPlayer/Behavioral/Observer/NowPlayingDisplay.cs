namespace MusicPlayer.Behavioral.Observer;

/// <summary>OBSERVER - concrete observer: prints every event as one line (the player's "screen").</summary>
public sealed class NowPlayingDisplay : IPlaybackObserver
{
    private readonly TextWriter _writer;

    public NowPlayingDisplay(TextWriter writer)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    public void Update(PlaybackEvent playbackEvent)
    {
        string label = playbackEvent.Kind switch
        {
            PlaybackEventKind.StateChanged => "player",
            PlaybackEventKind.TrackStarted => "now playing",
            PlaybackEventKind.VolumeChanged => "volume",
            PlaybackEventKind.OrderChanged => "order",
            PlaybackEventKind.ActionRejected => "rejected",
            _ => "event",
        };

        _writer.WriteLine($"   [{label}] {playbackEvent.Message}");
    }
}
