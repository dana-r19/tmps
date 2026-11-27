using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Behavioral.Strategy;

/// <summary>STRATEGY - concrete strategy: last queued item first.</summary>
public sealed class ReverseOrder : IPlaybackOrder
{
    public string Name => "reverse";

    public IReadOnlyList<IPlayable> Arrange(IReadOnlyList<IPlayable> items)
    {
        return items.Reverse().ToList();
    }
}
