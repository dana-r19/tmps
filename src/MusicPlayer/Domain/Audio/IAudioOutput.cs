namespace MusicPlayer.Domain.Audio;

/// <summary>
/// Where "sound" goes. Everything above this interface (playlists, decorators, the facade)
/// depends only on it, so a console, a test recorder or a real audio device can be plugged in.
/// </summary>
public interface IAudioOutput
{
    int Volume { get; set; }          // 0-100
    int FadeInSeconds { get; set; }   // 0 = no fade-in

    void Announce(string message);
    /// <param name="filePath">The audio file, if the song came from a real file; outputs that
    /// do not produce sound ignore it.</param>
    void Render(string trackName, int seconds, string? filePath);
}
