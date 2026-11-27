using MusicPlayer.Domain.Audio;

namespace MusicPlayer.Structural.Composite;

// =====================================================================================
// STRUCTURAL PATTERN: COMPOSITE
// -------------------------------------------------------------------------------------
// What it is:
//   Composite arranges objects into a tree (part-whole hierarchy) and lets the client
//   treat a single object and a group of objects in exactly the same way.
//
// How it is used here:
//   - IPlayable  = Component  (the shared interface, this file)
//   - Track      = Leaf       (one song, has no children)
//   - Playlist   = Composite  (holds IPlayable children: tracks or other playlists)
//
// Why:
//   A playlist can contain songs AND other playlists, nested to any depth. Because both
//   implement IPlayable, code that calls Play(), DurationSeconds or TrackCount never
//   needs to check "is this a song or a playlist?" - the tree handles it recursively.
// =====================================================================================

/// <summary>
/// COMPOSITE - the Component. A single track and a whole playlist are used through this same
/// interface. It is also the interface the Decorators wrap.
/// </summary>
public interface IPlayable
{
    string Name { get; }

    // For a Track: the song's length. For a Playlist: the sum of all its children.
    int DurationSeconds { get; }

    // For a Track: always 1. For a Playlist: counted recursively through nested playlists.
    int TrackCount { get; }

    // The operation shared by leaves and composites. Track plays itself;
    // Playlist forwards the call to each of its children.
    void Play(IAudioOutput output);
}
