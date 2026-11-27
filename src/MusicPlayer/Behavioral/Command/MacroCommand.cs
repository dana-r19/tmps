namespace MusicPlayer.Behavioral.Command;

/// <summary>
/// COMMAND - several undoable commands run as one (for example "party mode" = shuffle + loud).
/// One Undo reverses all of them, in reverse order. If a step is rejected, the steps already
/// done are undone, so the macro either happens completely or not at all.
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
        for (int i = _steps.Count - 1; i >= 0; i--)
        {
            _steps[i].Undo();
        }
    }
}
