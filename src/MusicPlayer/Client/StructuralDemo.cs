using MusicPlayer.Creational;
using MusicPlayer.Domain.Audio;
using MusicPlayer.Structural.Facade;

namespace MusicPlayer.Client;

/// <summary>The Lab 2 demo (Composite, Decorator, Facade), unchanged. Run with --structural.</summary>
internal static class StructuralDemo
{
    public static void Run()
    {
        IAudioOutput output = new ConsoleAudioOutput(PlayerSettings.Instance.DefaultVolume);
        IMusicPlayer player = new MusicPlayerFacade(output, SongLoaderRegistry.CreateDefault());

        Console.WriteLine("1) Import files (Factory Method picks a loader from the extension)");
        string loveStory = player.ImportSong("Taylor Swift - Love Story.mp3", 235);
        string shakeItOff = player.ImportSong("Taylor Swift - Shake It Off.mp3", 219);
        string blankSpace = player.ImportSong("Taylor Swift - Blank Space.ogg", 231);
        string demo = player.ImportSong("Demo Artist - Going Up.wav", 9);
        foreach (string name in player.Library)
        {
            Console.WriteLine($"   {name}");
        }

        Console.WriteLine();
        Console.WriteLine("2) Decorate (effects wrap an item and can be stacked in any order)");
        string quietLoveStory = player.WithVolume(loveStory, 50);
        string fadingBlankSpace = player.WithFadeIn(blankSpace);
        string loopedDemo = player.WithLoop(player.WithVolume(demo, 100), 2);
        Console.WriteLine($"   {quietLoveStory}");
        Console.WriteLine($"   {fadingBlankSpace}");
        Console.WriteLine($"   {loopedDemo}");

        Console.WriteLine();
        Console.WriteLine("3) Build playlists (Builder) and nest them (Composite)");
        player.CreatePlaylist("Pop Hits", quietLoveStory, shakeItOff);
        player.CreatePlaylist("Evening", fadingBlankSpace, "Pop Hits");
        player.AddToPlaylist("Evening", loopedDemo);
        Console.WriteLine("   Created 'Pop Hits' and 'Evening' (which contains 'Pop Hits')");

        Console.WriteLine();
        Console.WriteLine("4) Play 'Evening' (Facade -> Composite -> Decorators -> output)");
        player.Play("Evening");

        Console.WriteLine();
        Console.WriteLine("5) Decorate a whole playlist: 'Evening' at 30% volume, played twice");
        string background = player.WithLoop(player.WithVolume("Evening", 30), 2);
        player.Play(background);
    }
}
