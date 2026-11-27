using MusicPlayer.Domain.Audio;
using MusicPlayer.Domain.Models;

namespace MusicPlayer.Structural.Composite;

/// <summary>
/// COMPOSITE - the Composite (group). It holds IPlayable children, which can be tracks,
/// decorated tracks or other playlists, and treats them all the same way.
/// It is itself an IPlayable, so a playlist can be put inside another playlist - this is
/// what makes the structure a tree.
/// </summary>
public sealed class Playlist : IPlayable
{
    // Children are stored by the Component type (IPlayable), not by Track or Playlist,
    // so any kind of playable item can be added: leaf, composite or decorator.
    private readonly List<IPlayable> _children = new List<IPlayable>();

    public Playlist(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public IReadOnlyList<IPlayable> Children => _children;

    // Recursive aggregation: each child answers for itself (a Track returns its length,
    // a nested Playlist sums its own children), and the results are added up.
    public int DurationSeconds => _children.Sum(child => child.DurationSeconds);

    public int TrackCount => _children.Sum(child => child.TrackCount);

    // Child-management methods exist only on the Composite, not on the Component,
    // because a Track (leaf) cannot have children.
    public void Add(IPlayable item)
    {
        // Guard against the simplest cycle: a playlist inside itself would recurse forever.
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

    // Composite behaviour: announce the group, then delegate Play() to every child.
    // The playlist does not know or care whether a child is a song, a decorated song
    // or another playlist - they all just receive the same Play() call.
    public void Play(IAudioOutput output)
    {
        output.Announce($"Playlist '{Name}': {TrackCount} track(s), {TimeFormat.Format(DurationSeconds)}");
        foreach (IPlayable child in _children)
        {
            child.Play(output);
        }
    }
}
