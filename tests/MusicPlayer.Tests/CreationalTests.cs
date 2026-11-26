using System.Reflection;
using MusicPlayer.Creational;
using MusicPlayer.Domain.Models;

namespace MusicPlayer.Tests;

public class CreationalTests
{
    [Fact]
    public void Singleton_Returns_The_Same_Instance_And_Has_No_Public_Constructor()
    {
        Assert.Same(PlayerSettings.Instance, PlayerSettings.Instance);
        Assert.Empty(typeof(PlayerSettings).GetConstructors(BindingFlags.Public | BindingFlags.Instance));
    }

    [Theory]
    [InlineData("Artist - Song.mp3", typeof(Mp3Song))]
    [InlineData("Artist - Song.wav", typeof(WavSong))]
    [InlineData("Artist - Song.OGG", typeof(OggSong))]
    public void Factory_Method_Creates_The_Right_Song_For_Each_Extension(string fileName, Type expected)
    {
        Song song = SongLoaderRegistry.CreateDefault().Load(fileName, 60);

        Assert.IsType(expected, song);
        Assert.Equal("Artist", song.Artist);
        Assert.Equal("Song", song.Title);
    }

    [Fact]
    public void Factory_Method_Uses_Unknown_Artist_When_The_Name_Has_No_Separator()
    {
        Song song = new Mp3SongLoader().Load("JustATitle.mp3", 60);

        Assert.Equal("Unknown", song.Artist);
        Assert.Equal("JustATitle", song.Title);
    }

    [Fact]
    public void A_New_Format_Can_Be_Registered_Without_Editing_Existing_Classes()
    {
        SongLoaderRegistry registry = SongLoaderRegistry.CreateDefault().Register(".flac", new FlacSongLoader());

        Song song = registry.Load("Artist - Song.flac", 60);

        Assert.Equal("FLAC", song.Format);
    }

    [Fact]
    public void Builder_Keeps_Order_And_Can_Be_Reused_After_Build()
    {
        PlaylistBuilder builder = new PlaylistBuilder(SongLoaderRegistry.CreateDefault());

        var first = builder.Named("First").AddFile("A - One.mp3", 10).AddFile("B - Two.wav", 20).Build();
        var second = builder.AddFile("C - Three.ogg", 30).Build();

        Assert.Equal("First", first.Name);
        Assert.Equal(new[] { "One - A [MP3]", "Two - B [WAV]" }, first.Children.Select(c => c.Name));
        Assert.Equal("Untitled", second.Name);
        Assert.Equal(new[] { "Three - C [OGG]" }, second.Children.Select(c => c.Name));
    }

    // Test-only format: shows the Factory Method is open for extension.
    private sealed class FlacSong : Song
    {
        public FlacSong(string title, string artist, int durationSeconds)
            : base(title, artist, durationSeconds) { }

        public override string Format => "FLAC";
    }

    private sealed class FlacSongLoader : SongLoader
    {
        protected override Song CreateSong(string title, string artist, int durationSeconds)
        {
            return new FlacSong(title, artist, durationSeconds);
        }
    }
}
