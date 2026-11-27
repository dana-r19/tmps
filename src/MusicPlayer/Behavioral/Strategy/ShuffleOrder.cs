using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Behavioral.Strategy;

/// <summary>
/// STRATEGY - concrete strategy: random order (Fisher-Yates shuffle on a copy).
/// The Random can be passed in, so tests and the demo can use a fixed seed and get the
/// same "random" order every run.
/// </summary>
public sealed class ShuffleOrder : IPlaybackOrder
{
    private readonly Random _random;

    public ShuffleOrder() : this(new Random()) { }   // truly random (interactive mode)

    public ShuffleOrder(Random random)                // fixed seed (tests, demo)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
    }

    public string Name => "shuffle";

    public IReadOnlyList<IPlayable> Arrange(IReadOnlyList<IPlayable> items)
    {
        List<IPlayable> result = items.ToList();   // shuffle a copy, never the input

        // Fisher-Yates: walk from the end, swap each item with a random one at or before it.
        // Every order is equally likely and every item appears exactly once.
        for (int i = result.Count - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (result[i], result[j]) = (result[j], result[i]);
        }

        return result;
    }
}
