namespace MusicPlayer.Behavioral.State;

/// <summary>
/// STATE - concrete state. Play resumes the same item (it is not started again), Next starts the
/// following item, Stop stops; Pause is rejected (already paused).
/// </summary>
public sealed class PausedState : PlayerStateBase
{
    public override string Name => "Paused";

    public override bool Play(PlaybackSession session)
    {
        session.TransitionTo(new PlayingState(), "resumed");
        return true;
    }

    public override bool Stop(PlaybackSession session)
    {
        session.TransitionTo(new StoppedState());
        return true;
    }

    public override bool Next(PlaybackSession session)
    {
        if (!session.MoveToNext())
        {
            session.TransitionTo(new StoppedState(), "end of queue");
            return true;
        }

        session.TransitionTo(new PlayingState());
        session.PlayCurrent();
        return true;
    }
}
