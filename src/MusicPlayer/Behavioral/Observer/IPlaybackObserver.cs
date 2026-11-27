namespace MusicPlayer.Behavioral.Observer;

/// <summary>OBSERVER - the observer interface. The subject only knows this, never a concrete observer.</summary>
public interface IPlaybackObserver
{
    void Update(PlaybackEvent playbackEvent);
}
