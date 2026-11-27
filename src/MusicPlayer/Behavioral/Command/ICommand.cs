namespace MusicPlayer.Behavioral.Command;

/// <summary>
/// COMMAND - the command interface. A user request turned into an object, so the invoker can
/// run it, log it and (if it is an IUndoableCommand) undo it, without knowing what it does.
/// </summary>
public interface ICommand
{
    string Description { get; }

    /// <summary>Returns false if the receiver rejected the request.</summary>
    bool Execute();
}

/// <summary>COMMAND - a command that remembers enough to reverse itself.</summary>
public interface IUndoableCommand : ICommand
{
    void Undo();
}
