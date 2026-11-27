namespace MusicPlayer.Behavioral.Command;

/// <summary>
/// COMMAND - the invoker (the player's "remote control"). It runs any ICommand, keeps a history
/// of everything that was requested, and keeps an undo stack of the undoable commands that
/// succeeded. It never knows what a command does.
/// </summary>
public sealed class CommandInvoker
{
    private readonly Stack<IUndoableCommand> _undoStack = new Stack<IUndoableCommand>();
    private readonly List<string> _history = new List<string>();

    public IReadOnlyList<string> History => _history;

    public bool CanUndo => _undoStack.Count > 0;

    public bool Execute(ICommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        bool done = command.Execute();
        _history.Add(done ? command.Description : $"{command.Description} (rejected)");

        if (done && command is IUndoableCommand undoable)
        {
            _undoStack.Push(undoable);
        }

        return done;
    }

    /// <summary>Undoes the most recent undoable command. Returns false if there is nothing to undo.</summary>
    public bool Undo()
    {
        if (!_undoStack.TryPop(out IUndoableCommand? command))
        {
            return false;
        }

        command.Undo();
        _history.Add($"undo {command.Description}");
        return true;
    }
}
