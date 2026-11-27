using MusicPlayer.Domain.Audio;
using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Structural.Decorator;

/// <summary>
/// DECORATOR - concrete decorator: plays the wrapped item at a fixed volume.
/// Adds behaviour BEFORE and AFTER the wrapped call (set volume, play, restore volume)
/// without changing the Track or Playlist classes.
/// </summary>
public sealed class VolumeDecorator : PlayableDecorator
{
    private readonly int _percent;

    public VolumeDecorator(IPlayable inner, int percent) : base(inner)
    {
        if (percent < 0 || percent > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(percent), "Volume must be between 0 and 100.");
        }

        _percent = percent;
    }

    // Extends the wrapped name, so stacked decorators read like "Song (vol 50%) (x2)".
    // Duration and track count are not overridden: volume does not change them.
    public override string Name => $"{Inner.Name} (vol {_percent}%)";

    public override void Play(IAudioOutput output)
    {
        int previous = output.Volume;
        output.Volume = _percent;            // extra behaviour before
        try
        {
            Inner.Play(output);              // delegate to the wrapped object
        }
        finally
        {
            output.Volume = previous;        // extra behaviour after: restore, so the effect
                                             // applies only to the wrapped item
        }
    }
}
