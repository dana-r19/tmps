using MusicPlayer.Behavioral.State;

namespace MusicPlayer.Behavioral.Command;

/// <summary>
/// COMMAND - concrete undoable command. Execute remembers the volume before the change;
/// Undo puts it back. Changing the volume itself is the receiver's job.
/// </summary>
public sealed class SetVolumeCommand : IUndoableCommand
{
    private readonly IPlaybackSession _session;   // the RECEIVER (an interface, not PlaybackSession)
    private readonly int _percent;                // the request's parameter
    private int _previous;                        // the state needed for Undo

    public SetVolumeCommand(IPlaybackSession session, int percent)
    {
        // Validate when the command is CREATED, so a bad value never reaches the invoker.
        if (percent < 0 || percent > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(percent), "Volume must be between 0 and 100.");
        }

        _session = session ?? throw new ArgumentNullException(nameof(session));
        _percent = percent;
    }

    public string Description => $"volume {_percent}%";

    public bool Execute()
    {
        _previous = _session.Volume;     // 1. remember what to go back to
        _session.SetVolume(_percent);    // 2. let the receiver do the work
        return true;
    }

    public void Undo()
    {
        _session.SetVolume(_previous);   // reverse: same receiver method, old value
    }
}
