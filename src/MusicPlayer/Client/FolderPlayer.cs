using MusicPlayer.Creational;
using MusicPlayer.Domain.Audio;
using MusicPlayer.Structural.Facade;

namespace MusicPlayer.Client;

/// <summary>Plays real audio files from a folder through the speakers (Lab 2, Windows). Run with --play [folder].</summary>
internal static class FolderPlayer
{
    public static void Run(string folder)
    {
        if (!Directory.Exists(folder))
        {
            Console.WriteLine($"Folder '{Path.GetFullPath(folder)}' not found.");
            return;
        }

        IAudioOutput output = new SpeakerAudioOutput(PlayerSettings.Instance.DefaultVolume);
        IMusicPlayer player = new MusicPlayerFacade(output, SongLoaderRegistry.CreateDefault());

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
}
