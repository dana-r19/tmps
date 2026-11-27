namespace MusicPlayer.Behavioral.State;

// =====================================================================================
// STATE PATTERN - overview
// -------------------------------------------------------------------------------------
// Intent: let an object change its behavior when its internal state changes. It looks as
//         if the object changed its class.
//
// Roles in this project:
//   Context         -> PlaybackSession (holds the current IPlayerState and delegates to it)
//   State           -> IPlayerState (this file)
//   Shared default  -> PlayerStateBase (rejects every action unless a state overrides it)
//   Concrete states -> StoppedState, PlayingState, PausedState
//
// Problem it solves here: "play", "pause", "stop" and "next" mean different things depending
// on whether the player is stopped, playing or paused, and some are forbidden (you cannot
// pause a stopped player). Without State, every method of PlaybackSession would contain
// "switch (state) { ... }", and adding a state would mean editing all of them.
//
// How it works:
//   1. The user calls session.Pause().
//   2. PlaybackSession does NOT decide anything; it calls _state.Pause(this).
//   3. The current state object decides: PlayingState switches to PausedState
//      (session.TransitionTo(new PausedState())), while StoppedState rejects the action.
//   4. The next call goes to the NEW state object, so the same button now behaves differently.
//
// Transitions (anything else is rejected):
//   Stopped --Play--> Playing          Playing --Pause--> Paused
//   Playing --Stop--> Stopped          Playing --Next--> Playing (next item) / Stopped (end)
//   Paused  --Play--> Playing (resume) Paused  --Next--> Playing (next item) / Stopped (end)
//   Paused  --Stop--> Stopped
// =====================================================================================

/// <summary>
/// STATE - the state interface. PlaybackSession delegates each user action to its current state,
/// which decides whether the action is allowed and what the next state is.
/// Returns true if the action was carried out, false if it was rejected.
/// </summary>
public interface IPlayerState
{
    string Name { get; }

    // Every method receives the context, so the state can ask it to move to another state
    // (TransitionTo) or to do the actual work (PlayCurrent, MoveToNext, ...).
    bool Play(PlaybackSession session);
    bool Pause(PlaybackSession session);
    bool Stop(PlaybackSession session);
    bool Next(PlaybackSession session);
}
