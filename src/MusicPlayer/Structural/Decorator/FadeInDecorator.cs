using MusicPlayer.Domain.Audio;
using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Structural.Decorator;

/// <summary>DECORATOR - concrete decorator: plays the wrapped item with a fade-in.</summary>
public sealed class FadeInDecorator : PlayableDecorator
{
    private readonly int _seconds;

    public FadeInDecorator(IPlayable inner, int seconds) : base(inner)
    {
        if (seconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seconds), "Fade-in cannot be negative.");
        }

        _seconds = seconds;
    }

    public override string Name => $"{Inner.Name} (fade-in {_seconds}s)";

    public override void Play(IAudioOutput output)
    {
        int previous = output.FadeInSeconds;
        output.FadeInSeconds = _seconds;
        try
        {
            Inner.Play(output);
        }
        finally
        {
            output.FadeInSeconds = previous;
        }
    }
}
