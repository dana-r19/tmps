namespace MusicPlayer.Behavioral.Observer;

/// <summary>
/// OBSERVER - the subject interface: observers can subscribe and unsubscribe at runtime.
/// Implemented by PlaybackSession (and inherited by IPlaybackSession, so clients can
/// subscribe through the same interface they use to control the player).
/// </summary>
public interface IPlaybackSubject
{
    /// <summary>Start receiving events. Subscribing the same observer twice has no effect.</summary>
    void Subscribe(IPlaybackObserver observer);

    /// <summary>Stop receiving events. Safe to call even from inside Update.</summary>
    void Unsubscribe(IPlaybackObserver observer);
}
