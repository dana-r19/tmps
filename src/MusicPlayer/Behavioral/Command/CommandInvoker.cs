namespace MusicPlayer.Behavioral.Command;

/// <summary>
/// COMMAND - the invoker (the player's "remote control"). It runs any ICommand, keeps a history
/// of everything that was requested, and keeps an undo stack of the undoable commands that
/// succeeded. It never knows what a command does, so new commands need no change here.
/// </summary>
public sealed class CommandInvoker
{
    // Stack = last in, first out: Undo always reverses the MOST RECENT change first.
    private readonly Stack<IUndoableCommand> _undoStack = new Stack<IUndoableCommand>();
    private readonly List<string> _history = new List<string>();   // log of every request

    public IReadOnlyList<string> History => _history;

    public bool CanUndo => _undoStack.Count > 0;

    public bool Execute(ICommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        // COMMAND: the invoker only calls Execute(); what happens is up to the command and its receiver.
        bool done = command.Execute();

        // Every request is logged, including rejected ones.
        _history.Add(done ? command.Description : $"{command.Description} (rejected)");

        // Only successful, undoable commands can be undone. A rejected command changed
        // nothing, so "undoing" it would be wrong.
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
            return false;   // empty stack: safe, nothing happens
        }

        command.Undo();
        _history.Add($"undo {command.Description}");
        return true;
    }
}
