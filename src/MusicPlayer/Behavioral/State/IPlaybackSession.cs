using MusicPlayer.Behavioral.Observer;
using MusicPlayer.Behavioral.Strategy;
using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Behavioral.State;

/// <summary>
/// What clients (commands, the shell, the demo) see of a playback session. It is the Receiver for
/// the commands and the Subject for the observers. The state machine stays hidden behind it.
/// </summary>
public interface IPlaybackSession : IPlaybackSubject
{
    string StateName { get; }
    IPlayable? CurrentItem { get; }

    /// <summary>The queue in the order it will be played.</summary>
    IReadOnlyList<IPlayable> Queue { get; }

    int Volume { get; }
    IPlaybackOrder Order { get; }

    bool Play();
    bool Pause();
    bool Stop();
    bool Next();

    void SetVolume(int percent);
    void SetOrder(IPlaybackOrder order);
}
