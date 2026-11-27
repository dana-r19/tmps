using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Behavioral.Observer;

public enum PlaybackEventKind
{
    StateChanged,
    TrackStarted,
    VolumeChanged,
    OrderChanged,
    ActionRejected,
}

/// <summary>
/// OBSERVER - what the subject sends to its observers. It is an immutable copy of the facts, so an
/// observer never needs a reference back to the player (and cannot start an update loop by
/// changing it while being notified).
/// </summary>
public sealed record PlaybackEvent(
    PlaybackEventKind Kind,
    string Message,
    string State,
    IPlayable? Track,
    int Volume);
