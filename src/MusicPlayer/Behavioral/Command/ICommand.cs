namespace MusicPlayer.Behavioral.Command;

// =====================================================================================
// COMMAND PATTERN - overview
// -------------------------------------------------------------------------------------
// Intent: turn a request into an object, so it can be passed around, queued, logged
//         and undone.
//
// Roles in this project:
//   Command           -> ICommand (this file), IUndoableCommand (adds Undo)
//   Concrete commands -> PlayCommand, PauseCommand, StopCommand, NextCommand (not undoable),
//                        SetVolumeCommand, ChangeOrderCommand, MacroCommand (undoable)
//   Invoker           -> CommandInvoker (runs commands, keeps history and the undo stack)
//   Receiver          -> IPlaybackSession (does the real work)
//   Client            -> BehavioralDemo / InteractiveShell (create commands, hand them to the invoker)
//
// Problem it solves here: user actions come from different places (the scripted demo, the
// interactive shell, maybe a GUI later). They must be logged in a history, and settings
// changes must be undoable, even several at once ("party mode"). If the shell called
// session.SetVolume(...) directly, nobody would remember the old volume and nothing could be undone.
//
// How it works:
//   1. The client creates e.g. new SetVolumeCommand(session, 40).
//   2. It gives the command to invoker.Execute(command). The invoker knows only ICommand.
//   3. The command calls the receiver (session.SetVolume) and remembers the old value.
//   4. The invoker logs it and, if it is undoable and succeeded, pushes it on the undo stack.
//   5. invoker.Undo() pops the last command and calls its Undo().
// Rule: the business logic stays in the receiver. A command only calls it and remembers
// enough to reverse the call.
// =====================================================================================

/// <summary>
/// COMMAND - the command interface. A user request turned into an object, so the invoker can
/// run it, log it and (if it is an IUndoableCommand) undo it, without knowing what it does.
/// </summary>
public interface ICommand
{
    /// <summary>Short text for the history, e.g. "volume 40%".</summary>
    string Description { get; }

    /// <summary>
    /// Runs the request on the receiver. Returns false if the receiver rejected it (for example,
    /// the State does not allow "pause" while stopped).
    /// </summary>
    bool Execute();
}

/// <summary>
/// COMMAND - a command that remembers enough to reverse itself.
/// Kept as a separate interface, so commands that cannot be undone (play, next...) are not
/// forced to have a fake Undo method (Interface Segregation Principle).
/// </summary>
public interface IUndoableCommand : ICommand
{
    void Undo();
}
