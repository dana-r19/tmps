using MusicPlayer.Behavioral.Observer;
using MusicPlayer.Behavioral.Strategy;
using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Behavioral.State;

/// <summary>
/// What clients (commands, the shell, the demo) see of a playback session. It plays three roles:
///   - COMMAND: it is the Receiver; commands call these methods.
///   - OBSERVER: it is the Subject (inherits Subscribe/Unsubscribe from IPlaybackSubject).
///   - STATE: it is the Context, but the state machine stays hidden; clients only see StateName.
/// Clients depend on this interface, never on the PlaybackSession class.
/// </summary>
public interface IPlaybackSession : IPlaybackSubject
{
    string StateName { get; }
    IPlayable? CurrentItem { get; }

    /// <summary>The queue in the order it will be played (as arranged by the current Strategy).</summary>
    IReadOnlyList<IPlayable> Queue { get; }

    int Volume { get; }
    IPlaybackOrder Order { get; }

    // Transport actions: what they do depends on the current State.
    // They return false if the current State rejected the action.
    bool Play();
    bool Pause();
    bool Stop();
    bool Next();

    // Settings: valid in every state. Commands remember the old value to undo these.
    void SetVolume(int percent);
    void SetOrder(IPlaybackOrder order);
}
