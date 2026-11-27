using MusicPlayer.Domain.Audio;
using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Structural.Decorator;

/// <summary>
/// DECORATOR - concrete decorator: repeats the wrapped item several times.
/// Unlike the volume and fade-in decorators, this one also changes the item's data
/// (duration and track count), showing that a decorator can override any member.
/// </summary>
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

    // Overridden: a looped item lasts N times as long and plays N times as many tracks.
    public override int DurationSeconds => Inner.DurationSeconds * _times;

    public override int TrackCount => Inner.TrackCount * _times;

    // Extra behaviour: call the wrapped object's Play() repeatedly.
    public override void Play(IAudioOutput output)
    {
        for (int i = 0; i < _times; i++)
        {
            Inner.Play(output);
        }
    }
}
