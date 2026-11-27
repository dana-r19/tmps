using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Behavioral.Strategy;

/// <summary>
/// STRATEGY - the context. Holds the queued items and the current position, and asks its
/// IPlaybackOrder how to arrange them. It never checks which order it has, so the order can be
/// swapped while the player is running (ChangeOrder).
/// </summary>
public sealed class PlayQueue
{
    private readonly IReadOnlyList<IPlayable> _items;   // as queued; never reordered
    private IReadOnlyList<IPlayable> _arranged;         // as the current strategy wants them played
    private int _position;

    public PlayQueue(IEnumerable<IPlayable> items, IPlaybackOrder order)
    {
        _items = (items ?? throw new ArgumentNullException(nameof(items))).ToList();
        Order = order ?? throw new ArgumentNullException(nameof(order));
        _arranged = order.Arrange(_items);
    }

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
            return false;
        }

        _position++;
        return true;
    }

    public void Rewind()
    {
        _position = 0;
    }

    /// <summary>
    /// Swaps the strategy at runtime. The current item stays current; what comes after it
    /// follows the new order.
    /// </summary>
    public void ChangeOrder(IPlaybackOrder order)
    {
        IPlayable? current = Current;
        Order = order ?? throw new ArgumentNullException(nameof(order));
        _arranged = order.Arrange(_items);
        _position = current is null ? 0 : Math.Max(0, PositionOf(current));
    }

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
