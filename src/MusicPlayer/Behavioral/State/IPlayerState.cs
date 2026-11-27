namespace MusicPlayer.Behavioral.State;

/// <summary>
/// STATE - the state interface. PlaybackSession delegates each user action to its current state,
/// which decides whether the action is allowed and what the next state is.
/// Returns true if the action was carried out, false if it was rejected.
/// </summary>
public interface IPlayerState
{
    string Name { get; }

    bool Play(PlaybackSession session);
    bool Pause(PlaybackSession session);
    bool Stop(PlaybackSession session);
    bool Next(PlaybackSession session);
}
