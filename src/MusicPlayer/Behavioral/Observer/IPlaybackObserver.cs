namespace MusicPlayer.Behavioral.Observer;

// =====================================================================================
// OBSERVER PATTERN - overview
// -------------------------------------------------------------------------------------
// Intent: when one object (the subject) changes, all objects that depend on it (the
//         observers) are notified automatically, without the subject knowing who they are.
//
// Roles in this project:
//   Subject            -> IPlaybackSubject (Subscribe / Unsubscribe)
//   Concrete subject   -> PlaybackSession (keeps the observer list and calls Publish)
//   Observer           -> IPlaybackObserver (this file)
//   Concrete observers -> NowPlayingDisplay, ListeningStats, LoudVolumeWarning
//   Message            -> PlaybackEvent (what happened, as read-only data)
//
// Problem it solves here: the screen, the statistics and the hearing-safety warning all need
// to react when the state, track, volume or order changes. If PlaybackSession called them
// directly, it would depend on each one, and adding a new one (e.g. a scrobbler) would mean
// editing the session. The user can also switch observers on and off while the program runs
// ("watch stats" / "unwatch display" in interactive mode).
//
// How it works: PlaybackSession keeps a List<IPlaybackObserver>. After every change it builds
// one PlaybackEvent and calls Update(event) on each observer in the list (see
// PlaybackSession.Publish). It only knows the interface, never the concrete classes.
// =====================================================================================

/// <summary>OBSERVER - the observer interface. The subject only knows this, never a concrete observer.</summary>
public interface IPlaybackObserver
{
    /// <summary>
    /// Called by the subject after every change. Each observer decides for itself which
    /// event kinds it cares about and ignores the rest.
    /// </summary>
    void Update(PlaybackEvent playbackEvent);
}
