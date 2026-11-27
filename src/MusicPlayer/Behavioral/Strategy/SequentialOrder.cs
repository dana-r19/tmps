using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Behavioral.Strategy;

/// <summary>STRATEGY - concrete strategy: the order in which the items were queued.</summary>
public sealed class SequentialOrder : IPlaybackOrder
{
    public string Name => "sequential";

    public IReadOnlyList<IPlayable> Arrange(IReadOnlyList<IPlayable> items)
    {
        return items.ToList();
    }
}
