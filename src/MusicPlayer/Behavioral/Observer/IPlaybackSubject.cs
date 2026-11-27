namespace MusicPlayer.Behavioral.Observer;

/// <summary>OBSERVER - the subject interface: observers can subscribe and unsubscribe at runtime.</summary>
public interface IPlaybackSubject
{
    void Subscribe(IPlaybackObserver observer);
    void Unsubscribe(IPlaybackObserver observer);
}
