namespace MusicPlayer.Behavioral.State;

/// <summary>STATE - concrete state. Pause, Stop and Next are allowed; Play is rejected (already playing).</summary>
public sealed class PlayingState : PlayerStateBase
{
    public override string Name => "Playing";

    public override bool Pause(PlaybackSession session)
    {
        session.TransitionTo(new PausedState());
        return true;
    }

    public override bool Stop(PlaybackSession session)
    {
        session.TransitionTo(new StoppedState());
        return true;
    }

    public override bool Next(PlaybackSession session)
    {
        if (session.MoveToNext())
        {
            session.PlayCurrent();
        }
        else
        {
            session.TransitionTo(new StoppedState(), "end of queue");
        }

        return true;
    }
}
