using MusicPlayer.Domain.Audio;
using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Structural.Decorator;

/// <summary>
/// DECORATOR - the base decorator. It IS an IPlayable and HAS an IPlayable (the wrapped one),
/// so decorators can wrap tracks, playlists or other decorators, in any order.
/// By default it forwards everything to the wrapped object; concrete decorators override
/// only what they change.
/// </summary>
public abstract class PlayableDecorator : IPlayable
{
    protected PlayableDecorator(IPlayable inner)
    {
        Inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    protected IPlayable Inner { get; }

    public virtual string Name => Inner.Name;

    public virtual int DurationSeconds => Inner.DurationSeconds;

    public virtual int TrackCount => Inner.TrackCount;

    public abstract void Play(IAudioOutput output);
}
