using MusicPlayer.Domain.Audio;
using MusicPlayer.Domain.Models;

namespace MusicPlayer.Structural.Composite;

/// <summary>COMPOSITE - the Leaf: one song.</summary>
public sealed class Track : IPlayable
{
    private readonly Song _song;

    public Track(Song song)
    {
        _song = song;
    }

    public string Name => $"{_song.Title} - {_song.Artist} [{_song.Format}]";

    public int DurationSeconds => _song.DurationSeconds;

    public int TrackCount => 1;

    public void Play(IAudioOutput output)
    {
        output.Render(Name, DurationSeconds, _song.FilePath);
    }
}
