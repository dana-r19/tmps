using MusicPlayer.Domain.Models;
using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Creational;

/// <summary>
/// BUILDER (Lab 1): assembles a Playlist step by step. Every step returns the builder
/// so calls can be chained; Build() returns the result and resets the builder for reuse.
/// </summary>
public sealed class PlaylistBuilder
{
    private const string DefaultName = "Untitled";

    private readonly SongLoaderRegistry _loaders;
    private readonly List<IPlayable> _items = new List<IPlayable>();
    private string _name = DefaultName;

    public PlaylistBuilder(SongLoaderRegistry loaders)
    {
        _loaders = loaders;
    }

    public PlaylistBuilder Named(string name)
    {
        _name = name;
        return this;
    }

    public PlaylistBuilder AddSong(Song song)
    {
        _items.Add(new Track(song));
        return this;
    }

    /// <summary>Uses the Factory Method (through the registry) to create the song.</summary>
    public PlaylistBuilder AddFile(string fileName, int durationSeconds)
    {
        return AddSong(_loaders.Load(fileName, durationSeconds));
    }

    /// <summary>Adds any playable: a track, a decorated track or a whole playlist.</summary>
    public PlaylistBuilder Add(IPlayable item)
    {
        _items.Add(item);
        return this;
    }

    public Playlist Build()
    {
        Playlist playlist = new Playlist(_name);
        foreach (IPlayable item in _items)
        {
            playlist.Add(item);
        }

        _name = DefaultName;
        _items.Clear();
        return playlist;
    }
}
