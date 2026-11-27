namespace MusicPlayer.Behavioral.State;

/// <summary>
/// STATE - shared base: by default every action is rejected. A concrete state overrides only the
/// actions that are valid in it, so the allowed transitions are exactly the overridden methods.
/// </summary>
public abstract class PlayerStateBase : IPlayerState
{
    public abstract string Name { get; }

    public virtual bool Play(PlaybackSession session) => session.Reject("play");

    public virtual bool Pause(PlaybackSession session) => session.Reject("pause");

    public virtual bool Stop(PlaybackSession session) => session.Reject("stop");

    public virtual bool Next(PlaybackSession session) => session.Reject("skip to next");
}
