using MusicPlayer.Domain.Models;

namespace MusicPlayer.Domain.Audio;

/// <summary>Prints what would be played. No sound and no libraries needed.</summary>
public sealed class ConsoleAudioOutput : IAudioOutput
{
    public ConsoleAudioOutput(int initialVolume = 100)
    {
        Volume = initialVolume;
        FadeInSeconds = 0;
    }

    public int Volume { get; set; }
    public int FadeInSeconds { get; set; }

    public void Announce(string message)
    {
        Console.WriteLine(message);
    }

    public void Render(string trackName, int seconds, string? filePath)
    {
        string fade = FadeInSeconds > 0 ? $", fade-in {FadeInSeconds}s" : "";
        Console.WriteLine($"  > {trackName} [{TimeFormat.Format(seconds)}] volume {Volume}%{fade}");
    }
}
