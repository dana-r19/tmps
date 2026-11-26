using MusicPlayer.Domain.Audio;

namespace MusicPlayer.Tests;

/// <summary>
/// Test stub for IAudioOutput: records every Announce and Render call (with the volume and
/// fade-in in effect at that moment) so tests can check exactly what was played, and in what order.
/// </summary>
public sealed class RecordingAudioOutput : IAudioOutput
{
    public RecordingAudioOutput(int initialVolume = 100)
    {
        Volume = initialVolume;
    }

    public int Volume { get; set; }
    public int FadeInSeconds { get; set; }

    public List<string> Events { get; } = new List<string>();

    public List<string> Renders => Events.Where(e => e.StartsWith("render:", StringComparison.Ordinal)).ToList();

    public void Announce(string message)
    {
        Events.Add($"announce:{message}");
    }

    public void Render(string trackName, int seconds, string? filePath)
    {
        Events.Add($"render:{trackName}|{seconds}s|vol {Volume}|fade {FadeInSeconds}");
    }
}
