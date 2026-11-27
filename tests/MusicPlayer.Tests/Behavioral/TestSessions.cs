using MusicPlayer.Behavioral.State;
using MusicPlayer.Behavioral.Strategy;
using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Tests.Behavioral;

internal static class TestSessions
{
    /// <summary>A session over tracks A (10 s), B (30 s) and C (20 s), in sequential order.</summary>
    public static PlaybackSession ThreeTracks(RecordingAudioOutput output)
    {
        IPlayable[] items =
        {
            TestSongs.Track("A", 10),
            TestSongs.Track("B", 30),
            TestSongs.Track("C", 20),
        };
        return new PlaybackSession(output, items, new SequentialOrder());
    }

    public static string[] Titles(IEnumerable<IPlayable> items)
    {
        // "A - Tester [MP3]" -> "A"
        return items.Select(item => item.Name.Split(" - ")[0]).ToArray();
    }
}
