using MusicPlayer.Domain.Models;

namespace MusicPlayer.Behavioral.Observer;

/// <summary>
/// OBSERVER - concrete observer: counts what was listened to. It only reacts to TrackStarted
/// and ignores every other event. While it is unsubscribed it receives nothing, so it
/// simply stops counting (the demo shows this in step 6).
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

    // OBSERVER: called by the subject; this observer filters for the one event kind it needs.
    public void Update(PlaybackEvent playbackEvent)
    {
        if (playbackEvent.Kind != PlaybackEventKind.TrackStarted || playbackEvent.Track is null)
        {
            return;   // not interested in state/volume/order changes
        }

        // All data comes from the event itself; no need to ask the player anything.
        ItemsStarted++;
        SecondsQueued += playbackEvent.Track.DurationSeconds;
        _playsByItem[playbackEvent.Track.Name] = PlaysOf(playbackEvent.Track.Name) + 1;
    }

    public string Summary()
    {
        return $"{ItemsStarted} item(s) started, {TimeFormat.Format(SecondsQueued)} of music";
    }
}
