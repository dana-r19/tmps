namespace MusicPlayer.Behavioral.State;

/// <summary>
/// STATE - concrete state. Play resumes the same item (it is not started again), Next starts the
/// following item, Stop stops; Pause is rejected (already paused) by PlayerStateBase.
/// Note that "Play" here means something different than in StoppedState: that difference
/// in behavior is exactly what the State pattern is for.
/// </summary>
public sealed class PausedState : PlayerStateBase
{
    public override string Name => "Paused";

    // Paused --Play--> Playing (resume: no PlayCurrent call, so the track is not restarted)
    public override bool Play(PlaybackSession session)
    {
        session.TransitionTo(new PlayingState(), "resumed");
        return true;
    }

    // Paused --Stop--> Stopped
    public override bool Stop(PlaybackSession session)
    {
        session.TransitionTo(new StoppedState());
        return true;
    }

    // Paused --Next--> Playing (next item), or Stopped at the end of the queue.
    public override bool Next(PlaybackSession session)
    {
        if (!session.MoveToNext())
        {
            session.TransitionTo(new StoppedState(), "end of queue");
            return true;
        }

        session.TransitionTo(new PlayingState());   // skipping while paused starts playing again
        session.PlayCurrent();
        return true;
    }
}
