using MusicPlayer.Behavioral.Observer;

namespace MusicPlayer.Tests.Behavioral;

/// <summary>Test stub for IPlaybackObserver: records every event it receives.</summary>
public sealed class RecordingObserver : IPlaybackObserver
{
    public List<PlaybackEvent> Events { get; } = new List<PlaybackEvent>();

    public IEnumerable<PlaybackEventKind> Kinds => Events.Select(e => e.Kind);

    public void Update(PlaybackEvent playbackEvent)
    {
        Events.Add(playbackEvent);
    }
}
