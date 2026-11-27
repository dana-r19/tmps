using MusicPlayer.Behavioral.State;

namespace MusicPlayer.Behavioral.Command;

/// <summary>
/// COMMAND - concrete command for the "stop" button. The receiver is IPlaybackSession; whether the
/// action is allowed is decided by the session's current State, not by the command. Not undoable
/// (a track that was heard cannot be "un-heard"), but the invoker still logs it in its history.
/// </summary>
public sealed class StopCommand : ICommand
{
    private readonly IPlaybackSession _session;

    public StopCommand(IPlaybackSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    public string Description => "stop";

    public bool Execute() => _session.Stop();
}
