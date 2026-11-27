using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Behavioral.Strategy;

/// <summary>
/// STRATEGY - the context. Holds the queued items and the current position, and asks its
/// IPlaybackOrder how to arrange them. It never checks which order it has, so the order can be
/// swapped while the player is running (ChangeOrder).
/// PlaybackSession owns one PlayQueue and forwards "set order" requests to it.
/// </summary>
public sealed class PlayQueue
{
    private readonly IReadOnlyList<IPlayable> _items;   // as queued by the user; NEVER reordered
    private IReadOnlyList<IPlayable> _arranged;         // as the current strategy wants them played
    private int _position;                              // index into _arranged

    public PlayQueue(IEnumerable<IPlayable> items, IPlaybackOrder order)
    {
        _items = (items ?? throw new ArgumentNullException(nameof(items))).ToList();

        // STRATEGY: the context stores the strategy as the INTERFACE, not as a concrete class...
        Order = order ?? throw new ArgumentNullException(nameof(order));

        // ...and delegates the algorithm to it. No if/else on the kind of order.
        _arranged = order.Arrange(_items);
    }

    /// <summary>The strategy currently in use.</summary>
    public IPlaybackOrder Order { get; private set; }

    /// <summary>The items in the order they will be played.</summary>
    public IReadOnlyList<IPlayable> Items => _arranged;

    public bool IsEmpty => _arranged.Count == 0;

    public IPlayable? Current => IsEmpty ? null : _arranged[_position];

    /// <summary>Moves to the next item. Returns false (and stays put) at the end of the queue.</summary>
    public bool MoveNext()
    {
        if (_position + 1 >= _arranged.Count)
        {
            return false;   // end of queue: the State (PlayingState/PausedState) decides what to do
        }

        _position++;
        return true;
    }

    public void Rewind()
    {
        _position = 0;
    }

    /// <summary>
    /// STRATEGY swapped at runtime. The current item stays current; what comes after it
    /// follows the new order.
    /// </summary>
    public void ChangeOrder(IPlaybackOrder order)
    {
        IPlayable? current = Current;                      // remember what is playing now

        Order = order ?? throw new ArgumentNullException(nameof(order));   // swap the strategy
        _arranged = order.Arrange(_items);                 // always arrange the ORIGINAL list,
                                                           // so no item is ever lost or doubled

        // Find the current item in the new order, so playback continues from it.
        _position = current is null ? 0 : Math.Max(0, PositionOf(current));
    }

    // Compares by reference: the same object, not just an item with the same name.
    private int PositionOf(IPlayable item)
    {
        for (int i = 0; i < _arranged.Count; i++)
        {
            if (ReferenceEquals(_arranged[i], item))
            {
                return i;
            }
        }

        return -1;
    }
}
