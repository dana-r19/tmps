using MusicPlayer.Behavioral.State;
using MusicPlayer.Behavioral.Strategy;
using MusicPlayer.Structural.Composite;
using MusicPlayer.Structural.Decorator;

namespace MusicPlayer.Tests.Behavioral;

public class StrategyTests
{
    private static readonly IPlayable[] Items =
    {
        TestSongs.Track("A", 10),
        TestSongs.Track("B", 30),
        TestSongs.Track("C", 20),
    };

    [Fact]
    public void Each_Order_Arranges_The_Queue_As_Expected()
    {
        Assert.Equal(new[] { "A", "B", "C" }, TestSessions.Titles(new SequentialOrder().Arrange(Items)));
        Assert.Equal(new[] { "C", "B", "A" }, TestSessions.Titles(new ReverseOrder().Arrange(Items)));
        Assert.Equal(new[] { "A", "C", "B" }, TestSessions.Titles(new ShortestFirstOrder().Arrange(Items)));
    }

    [Fact]
    public void Shuffle_Is_A_Permutation_And_Repeatable_With_The_Same_Seed()
    {
        IReadOnlyList<IPlayable> first = new ShuffleOrder(new Random(42)).Arrange(Items);
        IReadOnlyList<IPlayable> second = new ShuffleOrder(new Random(42)).Arrange(Items);

        Assert.Equal(Items.OrderBy(i => i.Name), first.OrderBy(i => i.Name));   // same items, none lost or doubled
        Assert.Equal(first, second);                                          // same seed, same order
    }

    [Fact]
    public void Arranging_Does_Not_Modify_The_Input_List()
    {
        IPlayable[] copy = (IPlayable[])Items.Clone();

        new ReverseOrder().Arrange(copy);
        new ShuffleOrder(new Random(1)).Arrange(copy);

        Assert.Equal(Items, copy);
    }

    [Fact]
    public void Shortest_First_Measures_Decorated_And_Nested_Items_By_Their_Total_Length()
    {
        Playlist group = new Playlist("Group");                       // 5 + 5 = 10 s
        group.Add(TestSongs.Track("G1", 5));
        group.Add(TestSongs.Track("G2", 5));
        IPlayable looped = new LoopDecorator(TestSongs.Track("L", 4), 5);   // 4 x 5 = 20 s
        IPlayable single = TestSongs.Track("S", 15);

        IReadOnlyList<IPlayable> arranged = new ShortestFirstOrder().Arrange(new[] { looped, single, group });

        Assert.Equal(new[] { group, single, looped }, arranged);
    }

    [Fact]
    public void Swapping_The_Order_At_Runtime_Changes_What_Plays_Next()
    {
        RecordingAudioOutput output = new RecordingAudioOutput();
        PlaybackSession session = TestSessions.ThreeTracks(output);   // A(10) B(30) C(20)
        session.Play();                                               // A

        session.SetOrder(new ShortestFirstOrder());                   // A C B, A stays current
        session.Next();

        Assert.Contains("C - Tester", output.Renders[^1]);            // sequential would have played B
        Assert.Equal("shortest first", session.Order.Name);
        Assert.Equal(new[] { "A", "C", "B" }, TestSessions.Titles(session.Queue));
    }
}
