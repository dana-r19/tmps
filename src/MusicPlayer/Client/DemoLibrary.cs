using MusicPlayer.Structural.Facade;

namespace MusicPlayer.Client;

/// <summary>
/// Fills the library through the facade (Lab 1 + Lab 2 patterns) and returns the names to queue.
/// The queue mixes all kinds of IPlayable: a decorated song, a plain song, a faded song and a
/// whole playlist that contains a looped song.
/// </summary>
internal static class DemoLibrary
{
    public static string[] Create(IMusicPlayer player)
    {
        string loveStory = player.ImportSong("Taylor Swift - Love Story.mp3", 235);
        string shakeItOff = player.ImportSong("Taylor Swift - Shake It Off.mp3", 219);
        string blankSpace = player.ImportSong("Taylor Swift - Blank Space.ogg", 231);
        string goingUp = player.ImportSong("Demo Artist - Going Up.wav", 9);
        string wobble = player.ImportSong("Demo Artist - Wobble.wav", 12);

        string quietLoveStory = player.WithVolume(loveStory, 50);                       // Decorator
        string fadingBlankSpace = player.WithFadeIn(blankSpace);                        // Decorator + Singleton default
        string demos = player.CreatePlaylist("Demo Loops", player.WithLoop(goingUp, 2), wobble);   // Builder + Composite

        return new[] { quietLoveStory, shakeItOff, fadingBlankSpace, demos };
    }
}
