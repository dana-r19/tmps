using MusicPlayer.Domain.Audio;
using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Structural.Decorator;

/// <summary>DECORATOR - concrete decorator: repeats the wrapped item several times.</summary>
public sealed class LoopDecorator : PlayableDecorator
{
    private readonly int _times;

    public LoopDecorator(IPlayable inner, int times) : base(inner)
    {
        if (times < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(times), "Loop count must be at least 1.");
        }

        _times = times;
    }

    public override string Name => $"{Inner.Name} (x{_times})";

    public override int DurationSeconds => Inner.DurationSeconds * _times;

    public override int TrackCount => Inner.TrackCount * _times;

    public override void Play(IAudioOutput output)
    {
        for (int i = 0; i < _times; i++)
        {
            Inner.Play(output);
        }
    }
}
