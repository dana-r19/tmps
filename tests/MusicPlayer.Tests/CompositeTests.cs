using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Tests;

public class CompositeTests
{
    [Fact]
    public void Leaf_And_Group_Are_Used_Through_The_Same_Interface()
    {
        Playlist playlist = new Playlist("Group");
        playlist.Add(TestSongs.Track("A", 10));
        playlist.Add(TestSongs.Track("B", 20));

        IPlayable[] items = { TestSongs.Track("Solo", 15), playlist };

        Assert.Equal(15, items[0].DurationSeconds);
        Assert.Equal(1, items[0].TrackCount);
        Assert.Equal(30, items[1].DurationSeconds);
        Assert.Equal(2, items[1].TrackCount);
    }

    [Fact]
    public void Nested_Playlist_Totals_Are_The_Sum_Of_All_Leaves()
    {
        Playlist inner = new Playlist("Inner");
        inner.Add(TestSongs.Track("A", 10));
        inner.Add(TestSongs.Track("B", 20));

        Playlist outer = new Playlist("Outer");
        outer.Add(inner);
        outer.Add(TestSongs.Track("C", 30));
        outer.Add(TestSongs.Track("D", 40));

        Assert.Equal(100, outer.DurationSeconds);
        Assert.Equal(4, outer.TrackCount);
    }

    [Fact]
    public void Playing_A_Nested_Playlist_Renders_Leaves_In_Order()
    {
        Playlist inner = new Playlist("Inner");
        inner.Add(TestSongs.Track("B", 20));

        Playlist outer = new Playlist("Outer");
        outer.Add(TestSongs.Track("A", 10));
        outer.Add(inner);
        outer.Add(TestSongs.Track("C", 30));

        RecordingAudioOutput output = new RecordingAudioOutput();
        outer.Play(output);

        Assert.Equal(new[]
        {
            "announce:Playlist 'Outer': 3 track(s), 1:00",
            "render:A - Tester [MP3]|10s|vol 100|fade 0",
            "announce:Playlist 'Inner': 1 track(s), 0:20",
            "render:B - Tester [MP3]|20s|vol 100|fade 0",
            "render:C - Tester [MP3]|30s|vol 100|fade 0",
        }, output.Events);
    }

    [Fact]
    public void A_Playlist_Cannot_Contain_Itself()
    {
        Playlist playlist = new Playlist("Loop");

        Assert.Throws<ArgumentException>(() => playlist.Add(playlist));
    }
}
