using MusicPlayer.Domain.Audio;
using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Structural.Decorator;

// =====================================================================================
// STRUCTURAL PATTERN: DECORATOR
// -------------------------------------------------------------------------------------
// What it is:
//   Decorator adds new behaviour to an object at runtime by wrapping it in another object
//   that has the same interface. The wrapper does its extra work and forwards the rest
//   to the wrapped object.
//
// How it is used here:
//   - IPlayable          = Component          (interface shared with Composite)
//   - Track / Playlist   = Concrete Components (the objects being decorated)
//   - PlayableDecorator  = Base Decorator     (this file: holds the wrapped IPlayable)
//   - VolumeDecorator, FadeInDecorator, LoopDecorator = Concrete Decorators
//
// Why:
//   Effects (volume, fade-in, loop) can be combined freely, e.g.
//       new LoopDecorator(new VolumeDecorator(track, 50), 2)
//   Without Decorator we would need a subclass for every combination
//   (QuietTrack, LoopedQuietTrack, FadingLoopedPlaylist, ...). Because decorators are
//   IPlayable too, they work on single tracks AND on whole playlists.
// =====================================================================================

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

    // The wrapped object ("has-a"). It can be a Track, a Playlist or another decorator,
    // which is what allows decorators to be stacked.
    protected IPlayable Inner { get; }

    // Default behaviour: forward to the wrapped object unchanged.
    // Concrete decorators override only the members whose behaviour they change.
    public virtual string Name => Inner.Name;

    public virtual int DurationSeconds => Inner.DurationSeconds;

    public virtual int TrackCount => Inner.TrackCount;

    // Abstract: every concrete decorator must say what extra work it does around Inner.Play().
    public abstract void Play(IAudioOutput output);
}
