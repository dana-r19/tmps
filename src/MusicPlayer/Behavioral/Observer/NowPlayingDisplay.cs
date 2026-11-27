namespace MusicPlayer.Behavioral.Observer;

/// <summary>
/// OBSERVER - concrete observer: prints every event as one line (the player's "screen").
/// It writes to a TextWriter instead of Console directly, so the output can go anywhere
/// (console, file, or TextWriter.Null in tests).
/// </summary>
public sealed class NowPlayingDisplay : IPlaybackObserver
{
    private readonly TextWriter _writer;

    public NowPlayingDisplay(TextWriter writer)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    // OBSERVER: the subject calls this after every change. The display reacts to ALL events.
    public void Update(PlaybackEvent playbackEvent)
    {
        // Only chooses a label for printing; this is presentation, not playback logic.
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
