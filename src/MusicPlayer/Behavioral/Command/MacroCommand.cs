namespace MusicPlayer.Behavioral.Command;

/// <summary>
/// COMMAND - a macro: several undoable commands run as one (for example "party mode" =
/// shuffle + loud). One Undo reverses all of them, in reverse order. If a step is rejected,
/// the steps already done are undone, so the macro either happens completely or not at all.
/// A macro is itself an IUndoableCommand, so the invoker treats it like any other command.
/// </summary>
public sealed class MacroCommand : IUndoableCommand
{
    private readonly IReadOnlyList<IUndoableCommand> _steps;

    public MacroCommand(string name, params IUndoableCommand[] steps)
    {
        Description = name;
        _steps = steps.ToList();
    }

    public string Description { get; }

    public bool Execute()
    {
        for (int i = 0; i < _steps.Count; i++)
        {
            if (!_steps[i].Execute())
            {
                // Step i failed: roll back steps i-1 ... 0 (newest first), then report failure.
                for (int j = i - 1; j >= 0; j--)
                {
                    _steps[j].Undo();
                }

                return false;
            }
        }

        return true;
    }

    public void Undo()
    {
        // Reverse order: the last change is undone first, like a stack.
        for (int i = _steps.Count - 1; i >= 0; i--)
        {
            _steps[i].Undo();
        }
    }
}
