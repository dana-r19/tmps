using MusicPlayer.Behavioral.Command;
using MusicPlayer.Behavioral.Observer;
using MusicPlayer.Behavioral.State;
using MusicPlayer.Behavioral.Strategy;
using MusicPlayer.Creational;
using MusicPlayer.Domain.Audio;
using MusicPlayer.Structural.Facade;

namespace MusicPlayer.Client;

/// <summary>
/// The Lab 3 demo (the default run). Every user action is a Command run by the invoker; the
/// session's State decides what happens; Observers report it; the Strategy decides the order.
/// </summary>
internal static class BehavioralDemo
{
    public static void Run()
    {
        // Lab 2 FACADE builds the library and opens the session. From here on the demo only
        // uses abstractions: IPlaybackSession, ICommand, IPlaybackObserver, IPlaybackOrder.
        IAudioOutput output = new ConsoleAudioOutput(PlayerSettings.Instance.DefaultVolume);
        IMusicPlayer player = new MusicPlayerFacade(output, SongLoaderRegistry.CreateDefault());
        IPlaybackSession session = player.OpenSession(DemoLibrary.Create(player));   // STATE context + OBSERVER subject
        CommandInvoker remote = new CommandInvoker();                                 // COMMAND invoker

        Section("1) Queue built with the Lab 2 facade (songs, decorated songs, a nested playlist)");
        PrintQueue(session);

        Section("2) Observers subscribe at runtime");
        // OBSERVER: three independent observers. The session only sees IPlaybackObserver.
        // (stats is kept as its concrete type only so the demo can print its Summary at the end.)
        ListeningStats stats = new ListeningStats();
        IPlaybackObserver display = new NowPlayingDisplay(Console.Out);
        IPlaybackObserver loudWarning = new LoudVolumeWarning(85, Console.Out);
        session.Subscribe(display);
        session.Subscribe(stats);
        session.Subscribe(loudWarning);
        Console.WriteLine("   subscribed: display, stats, loud-volume warning");

        // COMMAND -> STATE -> OBSERVER: each button press is a command object run by the invoker.
        // The session's current state decides what it means, and the observers print the result.
        Section("3) Commands drive the State machine (Stopped -> Playing -> Paused -> Playing)");
        Run(remote, new PlayCommand(session));
        Run(remote, new PauseCommand(session));
        Run(remote, new PauseCommand(session));      // rejected by PausedState
        Run(remote, new PlayCommand(session));       // resumes, does not restart the track
        Run(remote, new NextCommand(session));

        Section("4) Undoable command: volume (the warning observer reacts)");
        Run(remote, new SetVolumeCommand(session, 95));
        Undo(remote);

        // COMMAND + STRATEGY: the command carries a new IPlaybackOrder; undo puts the old one back.
        Section("5) Strategy swapped at runtime: shortest first");
        Run(remote, new ChangeOrderCommand(session, new ShortestFirstOrder()));
        Run(remote, new NextCommand(session));
        Undo(remote);                                // back to sequential order

        Section("6) Stats observer unsubscribes; macro command 'party mode' (shuffle + volume 100)");
        session.Unsubscribe(stats);                  // OBSERVER: stats stops receiving events from now on
        Console.WriteLine("   unsubscribed: stats");
        Run(remote, new MacroCommand("party mode",
            new ChangeOrderCommand(session, new ShuffleOrder(new Random(7))),
            new SetVolumeCommand(session, 100)));
        Run(remote, new NextCommand(session));
        Undo(remote);                                // undoes both steps of the macro

        Section("7) Play to the end of the queue, then forbidden actions are rejected");
        Run(remote, new NextCommand(session));
        Run(remote, new NextCommand(session));       // the last item is a whole playlist (Composite)
        Run(remote, new NextCommand(session));       // Playing -> Stopped (end of queue)
        Run(remote, new PauseCommand(session));      // rejected by StoppedState
        Run(remote, new StopCommand(session));       // rejected by StoppedState

        Section("8) Command history (invoker) and listening stats (observer)");
        Console.WriteLine($"   history: {string.Join(" | ", remote.History)}");
        Console.WriteLine($"   stats: {stats.Summary()} (counted only while subscribed)");
    }

    // The client never calls session.Play() directly: every request goes through the invoker
    // as an ICommand, so it is logged (and undoable if it is an IUndoableCommand).
    private static void Run(CommandInvoker remote, ICommand command)
    {
        Console.WriteLine($"> {command.Description}");
        remote.Execute(command);
    }

    private static void Undo(CommandInvoker remote)
    {
        Console.WriteLine("> undo");
        remote.Undo();
    }

    private static void PrintQueue(IPlaybackSession session)
    {
        for (int i = 0; i < session.Queue.Count; i++)
        {
            Console.WriteLine($"   {i + 1}. {session.Queue[i].Name}");
        }
    }

    private static void Section(string title)
    {
        Console.WriteLine();
        Console.WriteLine(title);
    }
}
