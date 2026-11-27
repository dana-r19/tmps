namespace MusicPlayer.Behavioral.State;

/// <summary>STATE - concrete state. Only Play is allowed: it starts the queue from the beginning.</summary>
public sealed class StoppedState : PlayerStateBase
{
    public override string Name => "Stopped";

    public override bool Play(PlaybackSession session)
    {
        if (session.IsQueueEmpty)
        {
            return session.Reject("play", "the queue is empty");
        }

        session.Rewind();
        session.TransitionTo(new PlayingState());
        session.PlayCurrent();
        return true;
    }
}
