using MusicPlayer.Behavioral.Observer;
using MusicPlayer.Behavioral.Strategy;
using MusicPlayer.Domain.Audio;
using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Behavioral.State;

/// <summary>
/// The place where the behavioral patterns meet:
///   - STATE context: Play/Pause/Stop/Next are delegated to the current IPlayerState; there is
///     no switch on the state anywhere. The internal methods are the small operations the
///     states use to make a transition.
///   - OBSERVER subject: every change is published to the subscribed observers.
///   - STRATEGY: the queue order is delegated to PlayQueue, which uses an IPlaybackOrder.
///   - COMMAND receiver: commands call the public methods of IPlaybackSession.
/// It also uses Lab 2: the queue items are IPlayable (Composite + Decorator).
/// </summary>
public sealed class PlaybackSession : IPlaybackSession
{
    private readonly IAudioOutput _output;      // where "sound" goes (console, speakers, test stub)
    private readonly PlayQueue _queue;          // STRATEGY context
    private readonly List<IPlaybackObserver> _observers = new List<IPlaybackObserver>();   // OBSERVER list
    private IPlayerState _state = new StoppedState();   // STATE: a new session starts Stopped

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

    // ---- STATE: user actions are delegated to the current state ----
    // The context does not decide anything here. "this" is passed so the state can call back
    // (TransitionTo, PlayCurrent, ...). After a transition _state is a different object, so the
    // same method call behaves differently next time.

    public bool Play() => _state.Play(this);
    public bool Pause() => _state.Pause(this);
    public bool Stop() => _state.Stop(this);
    public bool Next() => _state.Next(this);

    // ---- settings that are valid in every state (no state involved) ----

    public void SetVolume(int percent)
    {
        if (percent < 0 || percent > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(percent), "Volume must be between 0 and 100.");
        }

        _output.Volume = percent;
        Publish(PlaybackEventKind.VolumeChanged, $"Volume set to {percent}%");   // OBSERVER: notify
    }

    public void SetOrder(IPlaybackOrder order)
    {
        _queue.ChangeOrder(order);   // STRATEGY: swap the algorithm at runtime
        string queue = string.Join(", ", _queue.Items.Select(item => item.Name));
        Publish(PlaybackEventKind.OrderChanged, $"Order '{order.Name}': {queue}");   // OBSERVER: notify
    }

    // ---- OBSERVER: the subject side ----

    public void Subscribe(IPlaybackObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        if (!_observers.Contains(observer))   // no duplicates: an observer is notified once per event
        {
            _observers.Add(observer);
        }
    }

    public void Unsubscribe(IPlaybackObserver observer)
    {
        _observers.Remove(observer);
    }

    // The "notify" loop of the Observer pattern. Called after every change.
    private void Publish(PlaybackEventKind kind, string message)
    {
        // One immutable snapshot for everyone, so all observers see the same facts.
        PlaybackEvent playbackEvent = new PlaybackEvent(kind, message, StateName, CurrentItem, Volume);

        // Iterate over a copy, so an observer may unsubscribe while it is being notified
        // (changing a List while a foreach runs over it would throw).
        foreach (IPlaybackObserver observer in _observers.ToArray())
        {
            observer.Update(playbackEvent);   // only the interface is known: no concrete observer here
        }
    }

    // ---- STATE: operations used by the states ----
    // "internal" = usable by the state classes (same assembly) but not part of IPlaybackSession,
    // so clients cannot jump between states on their own; only states make transitions.

    internal bool IsQueueEmpty => _queue.IsEmpty;

    /// <summary>Replaces the current state object. Called only by the states.</summary>
    internal void TransitionTo(IPlayerState next, string? reason = null)
    {
        string previous = _state.Name;
        _state = next;                                   // from now on, actions go to the new state
        string suffix = reason is null ? "" : $" ({reason})";
        Publish(PlaybackEventKind.StateChanged, $"{previous} -> {next.Name}{suffix}");   // OBSERVER
    }

    /// <summary>Plays the current queue item.</summary>
    internal void PlayCurrent()
    {
        IPlayable item = _queue.Current ?? throw new InvalidOperationException("The queue is empty.");
        Publish(PlaybackEventKind.TrackStarted, item.Name);   // OBSERVER
        item.Play(_output);   // Lab 2: Composite (a playlist plays its children) and Decorator (effects)
    }

    internal bool MoveToNext() => _queue.MoveNext();   // STRATEGY decides what "next" is

    internal void Rewind() => _queue.Rewind();

    /// <summary>Called by PlayerStateBase for forbidden actions. The state does not change.</summary>
    internal bool Reject(string action, string? reason = null)
    {
        string why = reason ?? $"the player is {StateName.ToLowerInvariant()}";
        Publish(PlaybackEventKind.ActionRejected, $"Cannot {action}: {why}");   // OBSERVER
        return false;   // the Command sees false, so the invoker does not put it on the undo stack
    }
}
