using MusicPlayer.Domain.Audio;
using MusicPlayer.Domain.Models;

namespace MusicPlayer.Structural.Composite;

/// <summary>
/// COMPOSITE - the Leaf: one song.
/// A leaf is the end of the tree: it has no children and does the real work itself.
/// It wraps a domain Song (created by the Factory Method) so the song can take part in
/// the IPlayable tree next to playlists and decorators.
/// </summary>
public sealed class Track : IPlayable
{
    private readonly Song _song;

    public Track(Song song)
    {
        _song = song;
    }

    public string Name => $"{_song.Title} - {_song.Artist} [{_song.Format}]";

    public int DurationSeconds => _song.DurationSeconds;

    // A leaf always counts as exactly one track.
    public int TrackCount => 1;

    // Leaf behaviour: no recursion, just send this one song to the audio output.
    public void Play(IAudioOutput output)
    {
        output.Render(Name, DurationSeconds, _song.FilePath);
    }
}
