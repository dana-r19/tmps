namespace MusicPlayer.Behavioral.State;

/// <summary>
/// STATE - concrete state. Only Play is allowed: it starts the queue from the beginning.
/// Pause, Stop and Next are not overridden, so PlayerStateBase rejects them.
/// </summary>
public sealed class StoppedState : PlayerStateBase
{
    public override string Name => "Stopped";

    // Stopped --Play--> Playing
    public override bool Play(PlaybackSession session)
    {
        if (session.IsQueueEmpty)
        {
            return session.Reject("play", "the queue is empty");   // nothing to play: stay Stopped
        }

        session.Rewind();                           // always start from the first item
        session.TransitionTo(new PlayingState());   // the transition is decided HERE, in the state
        session.PlayCurrent();                      // play the first item (Lab 2 Composite/Decorator)
        return true;
    }
}
