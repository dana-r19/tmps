using MusicPlayer.Domain.Audio;
using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Structural.Decorator;

/// <summary>DECORATOR - concrete decorator: plays the wrapped item at a fixed volume.</summary>
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

    public override string Name => $"{Inner.Name} (vol {_percent}%)";

    public override void Play(IAudioOutput output)
    {
        int previous = output.Volume;
        output.Volume = _percent;
        try
        {
            Inner.Play(output);
        }
        finally
        {
            output.Volume = previous;
        }
    }
}
