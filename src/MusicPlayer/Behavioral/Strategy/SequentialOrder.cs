using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Behavioral.Strategy;

/// <summary>
/// STRATEGY - concrete strategy: the order in which the items were queued.
/// This is the default order (MusicPlayerFacade.OpenSession uses it).
/// </summary>
public sealed class SequentialOrder : IPlaybackOrder
{
    public string Name => "sequential";

    public IReadOnlyList<IPlayable> Arrange(IReadOnlyList<IPlayable> items)
    {
        return items.ToList();   // a copy in the same order
    }
}
