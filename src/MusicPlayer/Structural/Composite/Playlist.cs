using MusicPlayer.Domain.Audio;
using MusicPlayer.Domain.Models;

namespace MusicPlayer.Structural.Composite;

/// <summary>
/// COMPOSITE - the Composite (group). It holds IPlayable children, which can be tracks,
/// decorated tracks or other playlists, and treats them all the same way.
/// </summary>
public sealed class Playlist : IPlayable
{
    private readonly List<IPlayable> _children = new List<IPlayable>();

    public Playlist(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public IReadOnlyList<IPlayable> Children => _children;

    public int DurationSeconds => _children.Sum(child => child.DurationSeconds);

    public int TrackCount => _children.Sum(child => child.TrackCount);

    public void Add(IPlayable item)
    {
        if (ReferenceEquals(item, this))
        {
            throw new ArgumentException("A playlist cannot contain itself.");
        }

        _children.Add(item);
    }

    public void Remove(IPlayable item)
    {
        _children.Remove(item);
    }

    public void Play(IAudioOutput output)
    {
        output.Announce($"Playlist '{Name}': {TrackCount} track(s), {TimeFormat.Format(DurationSeconds)}");
        foreach (IPlayable child in _children)
        {
            child.Play(output);
        }
    }
}
