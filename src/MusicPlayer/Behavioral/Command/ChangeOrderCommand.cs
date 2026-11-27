using MusicPlayer.Behavioral.State;
using MusicPlayer.Behavioral.Strategy;

namespace MusicPlayer.Behavioral.Command;

/// <summary>
/// COMMAND - concrete undoable command that swaps the play-order Strategy at runtime.
/// Undo restores the strategy that was active before.
/// This is where COMMAND and STRATEGY interact: the command carries a strategy object
/// and gives it to the receiver.
/// </summary>
public sealed class ChangeOrderCommand : IUndoableCommand
{
    private readonly IPlaybackSession _session;   // the RECEIVER
    private readonly IPlaybackOrder _order;       // the new STRATEGY to switch to
    private IPlaybackOrder? _previous;            // the strategy to go back to on Undo

    public ChangeOrderCommand(IPlaybackSession session, IPlaybackOrder order)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _order = order ?? throw new ArgumentNullException(nameof(order));
    }

    public string Description => $"order {_order.Name}";

    public bool Execute()
    {
        _previous = _session.Order;   // remember the old strategy object
        _session.SetOrder(_order);    // receiver swaps the strategy (and notifies observers)
        return true;
    }

    public void Undo()
    {
        if (_previous is not null)    // null only if Execute was never called
        {
            _session.SetOrder(_previous);   // the very same strategy instance as before
        }
    }
}
