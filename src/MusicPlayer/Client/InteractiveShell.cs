using MusicPlayer.Behavioral.Command;
using MusicPlayer.Behavioral.Observer;
using MusicPlayer.Behavioral.State;
using MusicPlayer.Behavioral.Strategy;

namespace MusicPlayer.Client;

/// <summary>
/// Lets the user change the player's behavior while it runs (advanced requirement A): switch the
/// play-order strategy, move between states, subscribe and unsubscribe observers, undo.
/// Every word the user types is looked up in a table and turned into a Command; the shell itself
/// has no playback logic. A new command, order or observer is one more line in a table below.
/// </summary>
internal sealed class InteractiveShell
{
    private readonly IPlaybackSession _session;
    private readonly TextReader _input;
    private readonly TextWriter _out;
    private readonly CommandInvoker _remote = new CommandInvoker();
    private readonly ListeningStats _stats = new ListeningStats();

    private readonly Dictionary<string, Func<string?, ICommand>> _commands;
    private readonly Dictionary<string, Func<IPlaybackOrder>> _orders;
    private readonly Dictionary<string, IPlaybackObserver> _observers;

    public InteractiveShell(IPlaybackSession session, TextReader input, TextWriter output)
    {
        _session = session;
        _input = input;
        _out = output;

        _orders = new Dictionary<string, Func<IPlaybackOrder>>(StringComparer.OrdinalIgnoreCase)
        {
            ["sequential"] = () => new SequentialOrder(),
            ["reverse"] = () => new ReverseOrder(),
            ["shuffle"] = () => new ShuffleOrder(),
            ["shortest"] = () => new ShortestFirstOrder(),
        };

        _observers = new Dictionary<string, IPlaybackObserver>(StringComparer.OrdinalIgnoreCase)
        {
            ["display"] = new NowPlayingDisplay(output),
            ["stats"] = _stats,
            ["loud"] = new LoudVolumeWarning(85, output),
        };

        _commands = new Dictionary<string, Func<string?, ICommand>>(StringComparer.OrdinalIgnoreCase)
        {
            ["play"] = _ => new PlayCommand(_session),
            ["pause"] = _ => new PauseCommand(_session),
            ["stop"] = _ => new StopCommand(_session),
            ["next"] = _ => new NextCommand(_session),
            ["vol"] = arg => new SetVolumeCommand(_session, int.Parse(arg ?? "")),
            ["order"] = arg => new ChangeOrderCommand(_session, Lookup(_orders, arg, "order")()),
            ["party"] = _ => new MacroCommand("party mode",
                new ChangeOrderCommand(_session, new ShuffleOrder()),
                new SetVolumeCommand(_session, 100)),
        };
    }

    public void Run()
    {
        _session.Subscribe(_observers["display"]);
        _out.WriteLine("Interactive player. Observer 'display' is on. Type 'help' for commands.");
        PrintQueue();

        while (true)
        {
            _out.Write($"[{_session.StateName}, vol {_session.Volume}%, {_session.Order.Name}] > ");
            string? line = _input.ReadLine();
            if (line is null)
            {
                _out.WriteLine();
                return;
            }

            string[] words = line.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
            {
                continue;
            }

            if (words[0].Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                Handle(words[0], words.Length > 1 ? words[1].Trim() : null);
            }
            catch (Exception error) when (error is FormatException or ArgumentException or KeyNotFoundException)
            {
                _out.WriteLine($"   {error.Message}");
            }
        }
    }

    private void Handle(string word, string? argument)
    {
        if (_commands.TryGetValue(word, out Func<string?, ICommand>? create))
        {
            _remote.Execute(create(argument));
            return;
        }

        switch (word.ToLowerInvariant())
        {
            case "undo":
                if (!_remote.Undo())
                {
                    _out.WriteLine("   nothing to undo");
                }
                break;
            case "watch":
                _session.Subscribe(Lookup(_observers, argument, "observer"));
                _out.WriteLine($"   '{argument}' subscribed");
                break;
            case "unwatch":
                _session.Unsubscribe(Lookup(_observers, argument, "observer"));
                _out.WriteLine($"   '{argument}' unsubscribed");
                break;
            case "queue":
                PrintQueue();
                break;
            case "history":
                _out.WriteLine($"   {string.Join(" | ", _remote.History)}");
                break;
            case "stats":
                _out.WriteLine($"   {_stats.Summary()}");
                break;
            case "help":
                PrintHelp();
                break;
            default:
                _out.WriteLine($"   unknown command '{word}' (type 'help')");
                break;
        }
    }

    private void PrintQueue()
    {
        for (int i = 0; i < _session.Queue.Count; i++)
        {
            string marker = ReferenceEquals(_session.Queue[i], _session.CurrentItem) ? "*" : " ";
            _out.WriteLine($"   {marker}{i + 1}. {_session.Queue[i].Name}");
        }
    }

    private void PrintHelp()
    {
        _out.WriteLine("   play | pause | stop | next          transport (the State decides if allowed)");
        _out.WriteLine("   vol <0-100>                         set volume (undoable)");
        _out.WriteLine($"   order <{string.Join("|", _orders.Keys)}>   change order (undoable)");
        _out.WriteLine("   party                               shuffle + volume 100 in one command (undoable)");
        _out.WriteLine("   undo                                undo the last undoable command");
        _out.WriteLine($"   watch|unwatch <{string.Join("|", _observers.Keys)}>   subscribe/unsubscribe an observer");
        _out.WriteLine("   queue | history | stats | help | quit");
    }

    private static T Lookup<T>(Dictionary<string, T> table, string? key, string what)
    {
        if (key is null || !table.TryGetValue(key, out T? value))
        {
            throw new KeyNotFoundException($"Unknown {what} '{key}'. Choose one of: {string.Join(", ", table.Keys)}");
        }

        return value;
    }
}
