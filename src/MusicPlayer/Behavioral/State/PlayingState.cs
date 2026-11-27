namespace MusicPlayer.Behavioral.State;

/// <summary>
/// STATE - concrete state. Pause, Stop and Next are allowed; Play is rejected (already playing),
/// because it is not overridden and PlayerStateBase rejects it.
/// </summary>
public sealed class PlayingState : PlayerStateBase
{
    public override string Name => "Playing";

    // Playing --Pause--> Paused
    public override bool Pause(PlaybackSession session)
    {
        session.TransitionTo(new PausedState());
        return true;
    }

    // Playing --Stop--> Stopped (the queue is rewound the next time Play is pressed)
    public override bool Stop(PlaybackSession session)
    {
        session.TransitionTo(new StoppedState());
        return true;
    }

    // Playing --Next--> Playing (next item), or Stopped at the end of the queue.
    public override bool Next(PlaybackSession session)
    {
        if (session.MoveToNext())        // which item is "next" is decided by the Strategy (PlayQueue)
        {
            session.PlayCurrent();       // still Playing: no transition needed
        }
        else
        {
            session.TransitionTo(new StoppedState(), "end of queue");
        }

        return true;
    }
}
