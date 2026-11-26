namespace MusicPlayer.Domain.Models;

/// <summary>
/// Product of the Factory Method (Lab 1). Plain data; each audio format is its own subclass.
/// </summary>
public abstract class Song
{
    protected Song(string title, string artist, int durationSeconds)
    {
        Title = title;
        Artist = artist;
        DurationSeconds = durationSeconds;
    }

    public string Title { get; }
    public string Artist { get; }
    public int DurationSeconds { get; }
    public abstract string Format { get; }

    /// <summary>The file the song was loaded from (set by SongLoader). Used for real playback.</summary>
    public string? FilePath { get; internal set; }
}

public sealed class Mp3Song : Song
{
    public Mp3Song(string title, string artist, int durationSeconds)
        : base(title, artist, durationSeconds) { }

    public override string Format => "MP3";
}

public sealed class WavSong : Song
{
    public WavSong(string title, string artist, int durationSeconds)
        : base(title, artist, durationSeconds) { }

    public override string Format => "WAV";
}

public sealed class OggSong : Song
{
    public OggSong(string title, string artist, int durationSeconds)
        : base(title, artist, durationSeconds) { }

    public override string Format => "OGG";
}
