using MusicPlayer.Creational;
using MusicPlayer.Structural.Facade;

namespace MusicPlayer.Tests;

public class FacadeTests
{
    private static (IMusicPlayer Player, RecordingAudioOutput Output) CreatePlayer()
    {
        RecordingAudioOutput output = new RecordingAudioOutput(100);
        return (new MusicPlayerFacade(output, SongLoaderRegistry.CreateDefault()), output);
    }

    [Fact]
    public void Import_Build_And_Play_Run_The_Subsystem_In_The_Right_Order()
    {
        var (player, output) = CreatePlayer();
        string one = player.ImportSong("Band - One.mp3", 10);
        string two = player.ImportSong("Band - Two.wav", 20);

        player.CreatePlaylist("Mix", one, two);
        player.Play("Mix");

        Assert.Equal(new[]
        {
            "announce:Playlist 'Mix': 2 track(s), 0:30",
            "render:One - Band [MP3]|10s|vol 100|fade 0",
            "render:Two - Band [WAV]|20s|vol 100|fade 0",
        }, output.Events);
    }

    [Fact]
    public void Facade_Passes_A_Decorated_Playlist_Through_Composite_And_Decorator()
    {
        var (player, output) = CreatePlayer();
        string one = player.ImportSong("Band - One.mp3", 10);
        string two = player.ImportSong("Band - Two.ogg", 20);
        player.CreatePlaylist("Mix", one, two);

        string quietMix = player.WithVolume("Mix", 50);
        player.Play(quietMix);

        Assert.Equal(2, output.Renders.Count);
        Assert.All(output.Renders, e => Assert.Contains("vol 50", e));
        Assert.Equal(100, output.Volume);
    }

    [Fact]
    public void Playing_An_Unknown_Item_Throws()
    {
        var (player, _) = CreatePlayer();

        Assert.Throws<KeyNotFoundException>(() => player.Play("Nothing"));
    }

    [Fact]
    public void Importing_An_Unsupported_Format_Throws()
    {
        var (player, _) = CreatePlayer();

        Assert.Throws<NotSupportedException>(() => player.ImportSong("Band - Lossless.flac", 10));
    }

    [Fact]
    public void Adding_To_A_Missing_Playlist_Throws()
    {
        var (player, _) = CreatePlayer();
        string one = player.ImportSong("Band - One.mp3", 10);

        // "one" is in the library, but it is a track, not a playlist.
        Assert.Throws<KeyNotFoundException>(() => player.AddToPlaylist(one, one));
    }
}
