using MusicPlayer.Behavioral.State;

namespace MusicPlayer.Behavioral.Command;

/// <summary>
/// COMMAND - concrete command for the "play" button. The receiver is IPlaybackSession; whether the
/// action is allowed is decided by the session's current State, not by the command. Not undoable
/// (a track that was heard cannot be "un-heard"), but the invoker still logs it in its history.
/// </summary>
public sealed class PlayCommand : ICommand
{
    private readonly IPlaybackSession _session;   // the RECEIVER (interface only)

    public PlayCommand(IPlaybackSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    public string Description => "play";

    // COMMAND -> STATE: the command only forwards the request. The session hands it to its
    // current state, which may reject it (returns false, so the invoker logs "(rejected)").
    public bool Execute() => _session.Play();
}
