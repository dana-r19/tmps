using MusicPlayer.Behavioral.State;
using MusicPlayer.Behavioral.Strategy;

namespace MusicPlayer.Behavioral.Command;

/// <summary>
/// COMMAND - concrete undoable command that swaps the play-order Strategy at runtime.
/// Undo restores the strategy that was active before.
/// </summary>
public sealed class ChangeOrderCommand : IUndoableCommand
{
    private readonly IPlaybackSession _session;
    private readonly IPlaybackOrder _order;
    private IPlaybackOrder? _previous;

    public ChangeOrderCommand(IPlaybackSession session, IPlaybackOrder order)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _order = order ?? throw new ArgumentNullException(nameof(order));
    }

    public string Description => $"order {_order.Name}";

    public bool Execute()
    {
        _previous = _session.Order;
        _session.SetOrder(_order);
        return true;
    }

    public void Undo()
    {
        if (_previous is not null)
        {
            _session.SetOrder(_previous);
        }
    }
}
