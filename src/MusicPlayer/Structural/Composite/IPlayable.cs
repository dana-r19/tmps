using MusicPlayer.Domain.Audio;

namespace MusicPlayer.Structural.Composite;

/// <summary>
/// COMPOSITE - the Component. A single track and a whole playlist are used through this same
/// interface. It is also the interface the Decorators wrap.
/// </summary>
public interface IPlayable
{
    string Name { get; }
    int DurationSeconds { get; }
    int TrackCount { get; }
    void Play(IAudioOutput output);
}
