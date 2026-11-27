using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Behavioral.Strategy;

/// <summary>
/// STRATEGY - concrete strategy: shortest item first. Uses IPlayable.DurationSeconds, so a whole
/// playlist or a looped track is measured by its total length (Composite and Decorator from Lab 2).
/// Items of equal length keep their queued order (OrderBy is stable).
/// </summary>
public sealed class ShortestFirstOrder : IPlaybackOrder
{
    public string Name => "shortest first";

    public IReadOnlyList<IPlayable> Arrange(IReadOnlyList<IPlayable> items)
    {
        return items.OrderBy(item => item.DurationSeconds).ToList();
    }
}
