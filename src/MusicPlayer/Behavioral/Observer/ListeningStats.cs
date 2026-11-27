using MusicPlayer.Domain.Models;

namespace MusicPlayer.Behavioral.Observer;

/// <summary>
/// OBSERVER - concrete observer: counts what was listened to. It only reacts to TrackStarted
/// and ignores every other event.
/// </summary>
public sealed class ListeningStats : IPlaybackObserver
{
    private readonly Dictionary<string, int> _playsByItem = new Dictionary<string, int>();

    public int ItemsStarted { get; private set; }
    public int SecondsQueued { get; private set; }

    public int PlaysOf(string itemName)
    {
        return _playsByItem.TryGetValue(itemName, out int count) ? count : 0;
    }

    public void Update(PlaybackEvent playbackEvent)
    {
        if (playbackEvent.Kind != PlaybackEventKind.TrackStarted || playbackEvent.Track is null)
        {
            return;
        }

        ItemsStarted++;
        SecondsQueued += playbackEvent.Track.DurationSeconds;
        _playsByItem[playbackEvent.Track.Name] = PlaysOf(playbackEvent.Track.Name) + 1;
    }

    public string Summary()
    {
        return $"{ItemsStarted} item(s) started, {TimeFormat.Format(SecondsQueued)} of music";
    }
}
