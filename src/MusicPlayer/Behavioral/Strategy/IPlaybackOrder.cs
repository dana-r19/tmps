using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Behavioral.Strategy;

/// <summary>
/// STRATEGY - the strategy interface. Decides in which order the queue is played.
/// Every order has this same signature, so PlayQueue can use any of them without knowing which.
/// </summary>
public interface IPlaybackOrder
{
    string Name { get; }

    /// <summary>Returns the items in the order they should be played. The input list is not modified.</summary>
    IReadOnlyList<IPlayable> Arrange(IReadOnlyList<IPlayable> items);
}
