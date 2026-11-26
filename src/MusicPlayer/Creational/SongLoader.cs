using MusicPlayer.Domain.Models;

namespace MusicPlayer.Creational;

/// <summary>
/// FACTORY METHOD (Lab 1) - the Creator.
/// Load() holds the shared logic (reading "Artist - Title.ext" from a file name) and calls the
/// factory method CreateSong(); each subclass decides WHICH Song subclass is created.
/// </summary>
public abstract class SongLoader
{
    public Song Load(string fileName, int durationSeconds)
    {
        string name = Path.GetFileNameWithoutExtension(fileName);
        string artist = "Unknown";
        string title = name;

        int separator = name.IndexOf(" - ", StringComparison.Ordinal);
        if (separator >= 0)
        {
            artist = name.Substring(0, separator).Trim();
            title = name.Substring(separator + 3).Trim();
        }

        Song song = CreateSong(title, artist, durationSeconds);
        song.FilePath = fileName;
        return song;
    }

    /// <summary>The factory method.</summary>
    protected abstract Song CreateSong(string title, string artist, int durationSeconds);
}

public sealed class Mp3SongLoader : SongLoader
{
    protected override Song CreateSong(string title, string artist, int durationSeconds)
    {
        return new Mp3Song(title, artist, durationSeconds);
    }
}

public sealed class WavSongLoader : SongLoader
{
    protected override Song CreateSong(string title, string artist, int durationSeconds)
    {
        return new WavSong(title, artist, durationSeconds);
    }
}

public sealed class OggSongLoader : SongLoader
{
    protected override Song CreateSong(string title, string artist, int durationSeconds)
    {
        return new OggSong(title, artist, durationSeconds);
    }
}

/// <summary>
/// Chooses the loader from the file extension. A new format = one new SongLoader subclass
/// plus one Register call; nothing existing is edited.
/// </summary>
public sealed class SongLoaderRegistry
{
    private readonly Dictionary<string, SongLoader> _loaders =
        new Dictionary<string, SongLoader>(StringComparer.OrdinalIgnoreCase);

    public SongLoaderRegistry Register(string extension, SongLoader loader)
    {
        _loaders[extension] = loader;
        return this;
    }

    public Song Load(string fileName, int durationSeconds)
    {
        string extension = Path.GetExtension(fileName);
        if (!_loaders.TryGetValue(extension, out SongLoader? loader))
        {
            throw new NotSupportedException($"No loader registered for '{extension}'.");
        }

        return loader.Load(fileName, durationSeconds);
    }

    public static SongLoaderRegistry CreateDefault()
    {
        return new SongLoaderRegistry()
            .Register(".mp3", new Mp3SongLoader())
            .Register(".wav", new WavSongLoader())
            .Register(".ogg", new OggSongLoader());
    }
}
