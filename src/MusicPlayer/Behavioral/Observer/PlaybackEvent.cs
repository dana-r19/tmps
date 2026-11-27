using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Behavioral.Observer;

/// <summary>What kind of change happened. Observers filter on this.</summary>
public enum PlaybackEventKind
{
    StateChanged,     // e.g. Stopped -> Playing (published by PlaybackSession.TransitionTo)
    TrackStarted,     // a queue item started playing (PlaybackSession.PlayCurrent)
    VolumeChanged,    // PlaybackSession.SetVolume
    OrderChanged,     // the Strategy was swapped (PlaybackSession.SetOrder)
    ActionRejected,   // the current State did not allow an action (PlaybackSession.Reject)
}

/// <summary>
/// OBSERVER - the message the subject sends to its observers ("push" model: the data travels
/// with the notification). It is an immutable record, a snapshot of the facts at that moment.
/// Observers never get a reference back to the player, so they cannot change it while being
/// notified, and an endless update loop is impossible.
/// </summary>
public sealed record PlaybackEvent(
    PlaybackEventKind Kind,
    string Message,       // human-readable description, e.g. "Playing -> Paused"
    string State,         // name of the state at the moment of the event
    IPlayable? Track,     // the current queue item (null if the queue is empty)
    int Volume);          // master volume at the moment of the event
