using MusicPlayer.Domain.Models;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace MusicPlayer.Domain.Audio;

/// <summary>
/// Plays real audio files through the speakers (NAudio, Windows). It is just another IAudioOutput,
/// so playlists and decorators work with it unchanged: they set Volume and FadeInSeconds,
/// and this class applies them to the sound.
/// Press S to skip the current track, Q to stop everything.
/// </summary>
public sealed class SpeakerAudioOutput : IAudioOutput
{
    private bool _stopRequested;

    public SpeakerAudioOutput(int initialVolume = 100)
    {
        Volume = initialVolume;
        FadeInSeconds = 0;
    }

    public int Volume { get; set; }
    public int FadeInSeconds { get; set; }

    public void Announce(string message)
    {
        if (!_stopRequested)
        {
            Console.WriteLine(message);
        }
    }

    public void Render(string trackName, int seconds, string? filePath)
    {
        if (_stopRequested)
        {
            return;
        }

        string fade = FadeInSeconds > 0 ? $", fade-in {FadeInSeconds}s" : "";
        Console.WriteLine($"  > {trackName} [{TimeFormat.Format(seconds)}] volume {Volume}%{fade}");

        if (filePath is null || !File.Exists(filePath))
        {
            Console.WriteLine("    (no audio file, skipped)");
            return;
        }

        using AudioFileReader reader = new AudioFileReader(filePath);
        FadeInOutSampleProvider faded = new FadeInOutSampleProvider(reader, initiallySilent: FadeInSeconds > 0);
        if (FadeInSeconds > 0)
        {
            faded.BeginFadeIn(FadeInSeconds * 1000);
        }

        VolumeSampleProvider volume = new VolumeSampleProvider(faded) { Volume = Volume / 100f };

        using WaveOutEvent device = new WaveOutEvent();
        device.Init(volume);
        device.Play();

        while (device.PlaybackState == PlaybackState.Playing)
        {
            ConsoleKey? key = ReadKey();
            if (key == ConsoleKey.S)
            {
                Console.WriteLine("    (skipped)");
                break;
            }

            if (key == ConsoleKey.Q)
            {
                Console.WriteLine("    (stopped)");
                _stopRequested = true;
                break;
            }

            Thread.Sleep(100);
        }

        device.Stop();
    }

    /// <summary>Reads the real length of an audio file, rounded to whole seconds.</summary>
    public static int GetDurationSeconds(string filePath)
    {
        using AudioFileReader reader = new AudioFileReader(filePath);
        return (int)Math.Round(reader.TotalTime.TotalSeconds);
    }

    private static ConsoleKey? ReadKey()
    {
        try
        {
            return Console.KeyAvailable ? Console.ReadKey(intercept: true).Key : null;
        }
        catch (InvalidOperationException)
        {
            return null; // input is redirected (no keyboard); just play through
        }
    }
}
