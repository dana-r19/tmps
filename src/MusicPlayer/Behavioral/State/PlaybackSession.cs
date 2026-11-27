using MusicPlayer.Behavioral.Observer;
using MusicPlayer.Behavioral.Strategy;
using MusicPlayer.Domain.Audio;
using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Behavioral.State;

/// <summary>
/// STATE - the context. Play/Pause/Stop/Next are delegated to the current IPlayerState; there is
/// no switch on the state anywhere. The internal methods are the small operations the states
/// use to make a transition.
/// It is also the OBSERVER subject: every change is published to the subscribed observers.
/// The queue order is delegated to PlayQueue (STRATEGY context).
/// </summary>
public sealed class PlaybackSession : IPlaybackSession
{
    private readonly IAudioOutput _output;
    private readonly PlayQueue _queue;
    private readonly List<IPlaybackObserver> _observers = new List<IPlaybackObserver>();
    private IPlayerState _state = new StoppedState();

    public PlaybackSession(IAudioOutput output, IEnumerable<IPlayable> items, IPlaybackOrder order)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _queue = new PlayQueue(items, order);
    }

    public string StateName => _state.Name;
    public IPlayable? CurrentItem => _queue.Current;
    public IReadOnlyList<IPlayable> Queue => _queue.Items;
    public int Volume => _output.Volume;
    public IPlaybackOrder Order => _queue.Order;

    // ---- user actions: delegated to the current state ----

    public bool Play() => _state.Play(this);
    public bool Pause() => _state.Pause(this);
    public bool Stop() => _state.Stop(this);
    public bool Next() => _state.Next(this);

    // ---- settings that are valid in every state ----

    public void SetVolume(int percent)
    {
        if (percent < 0 || percent > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(percent), "Volume must be between 0 and 100.");
        }

        _output.Volume = percent;
        Publish(PlaybackEventKind.VolumeChanged, $"Volume set to {percent}%");
    }

    public void SetOrder(IPlaybackOrder order)
    {
        _queue.ChangeOrder(order);
        string queue = string.Join(", ", _queue.Items.Select(item => item.Name));
        Publish(PlaybackEventKind.OrderChanged, $"Order '{order.Name}': {queue}");
    }

    // ---- OBSERVER: the subject side ----

    public void Subscribe(IPlaybackObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }
    }

    public void Unsubscribe(IPlaybackObserver observer)
    {
        _observers.Remove(observer);
    }

    private void Publish(PlaybackEventKind kind, string message)
    {
        PlaybackEvent playbackEvent = new PlaybackEvent(kind, message, StateName, CurrentItem, Volume);

        // Iterate over a copy, so an observer may unsubscribe while it is being notified.
        foreach (IPlaybackObserver observer in _observers.ToArray())
        {
            observer.Update(playbackEvent);
        }
    }

    // ---- operations used by the states (not part of the public interface) ----

    internal bool IsQueueEmpty => _queue.IsEmpty;

    internal void TransitionTo(IPlayerState next, string? reason = null)
    {
        string previous = _state.Name;
        _state = next;
        string suffix = reason is null ? "" : $" ({reason})";
        Publish(PlaybackEventKind.StateChanged, $"{previous} -> {next.Name}{suffix}");
    }

    internal void PlayCurrent()
    {
        IPlayable item = _queue.Current ?? throw new InvalidOperationException("The queue is empty.");
        Publish(PlaybackEventKind.TrackStarted, item.Name);
        item.Play(_output);   // Lab 2: Composite and Decorator do the actual playing
    }

    internal bool MoveToNext() => _queue.MoveNext();

    internal void Rewind() => _queue.Rewind();

    internal bool Reject(string action, string? reason = null)
    {
        string why = reason ?? $"the player is {StateName.ToLowerInvariant()}";
        Publish(PlaybackEventKind.ActionRejected, $"Cannot {action}: {why}");
        return false;
    }
}
