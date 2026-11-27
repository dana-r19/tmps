using MusicPlayer.Behavioral.State;

namespace MusicPlayer.Behavioral.Command;

/// <summary>
/// COMMAND - concrete undoable command. Execute remembers the volume before the change;
/// Undo puts it back. Changing the volume itself is the receiver's job.
/// </summary>
public sealed class SetVolumeCommand : IUndoableCommand
{
    private readonly IPlaybackSession _session;
    private readonly int _percent;
    private int _previous;

    public SetVolumeCommand(IPlaybackSession session, int percent)
    {
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
        _previous = _session.Volume;
        _session.SetVolume(_percent);
        return true;
    }

    public void Undo()
    {
        _session.SetVolume(_previous);
    }
}
