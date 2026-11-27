// CLIENT: talks only to IMusicPlayer (the Facade). It never touches loaders, builders,
// playlists or decorators directly.
//
// Where the structural patterns are:
//   Composite -> Structural/Composite  (IPlayable, Track, Playlist)
//   Decorator -> Structural/Decorator  (PlayableDecorator, Volume/FadeIn/Loop decorators)
//   Facade    -> Structural/Facade     (IMusicPlayer, MusicPlayerFacade)
//
//   dotnet run --project src/MusicPlayer                     console demo (no sound)
//   dotnet run --project src/MusicPlayer -- --play [folder]  play real files (default folder: songs)
using MusicPlayer.Creational;
using MusicPlayer.Domain.Audio;
using MusicPlayer.Structural.Facade;

PlayerSettings settings = PlayerSettings.Instance;

int playIndex = Array.IndexOf(args, "--play");
if (playIndex >= 0)
{
    string folder = playIndex + 1 < args.Length ? args[playIndex + 1] : "songs";
    PlayFolder(folder);
}
else
{
    RunConsoleDemo();
}

void PlayFolder(string folder)
{
    if (!Directory.Exists(folder))
    {
        Console.WriteLine($"Folder '{Path.GetFullPath(folder)}' not found.");
        return;
    }

    IMusicPlayer player = new MusicPlayerFacade(new SpeakerAudioOutput(settings.DefaultVolume), SongLoaderRegistry.CreateDefault());

    Console.WriteLine($"Importing songs from '{Path.GetFullPath(folder)}'");
    List<string> songs = new List<string>();
    foreach (string file in Directory.GetFiles(folder).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
    {
        try
        {
            songs.Add(player.ImportFile(file));
            Console.WriteLine($"   {songs[^1]}");
        }
        catch (NotSupportedException)
        {
            Console.WriteLine($"   (skipped {Path.GetFileName(file)}: unsupported format)");
        }
    }

    if (songs.Count == 0)
    {
        Console.WriteLine("No playable songs found.");
        return;
    }

    // Each song fades in (Decorator); all of them go into one playlist (Builder + Composite).
    string[] fadedSongs = songs.Select(song => player.WithFadeIn(song)).ToArray();
    player.CreatePlaylist("My Songs", fadedSongs);

    Console.WriteLine();
    Console.WriteLine("Playing. Press S to skip a track, Q to quit.");
    player.Play("My Songs");
}

void RunConsoleDemo()
{
    IAudioOutput output = new ConsoleAudioOutput(settings.DefaultVolume);
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
