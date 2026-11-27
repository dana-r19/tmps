using MusicPlayer.Behavioral.Command;
using MusicPlayer.Behavioral.State;
using MusicPlayer.Behavioral.Strategy;

namespace MusicPlayer.Tests.Behavioral;

public class CommandTests
{
    [Fact]
    public void SetVolume_Then_Undo_Restores_The_Original_Volume()
    {
        IPlaybackSession session = TestSessions.ThreeTracks(new RecordingAudioOutput(70));
        CommandInvoker invoker = new CommandInvoker();

        invoker.Execute(new SetVolumeCommand(session, 20));
        Assert.Equal(20, session.Volume);

        Assert.True(invoker.Undo());
        Assert.Equal(70, session.Volume);
    }

    [Fact]
    public void ChangeOrder_Then_Undo_Restores_The_Previous_Strategy()
    {
        IPlaybackSession session = TestSessions.ThreeTracks(new RecordingAudioOutput());
        IPlaybackOrder original = session.Order;
        CommandInvoker invoker = new CommandInvoker();

        invoker.Execute(new ChangeOrderCommand(session, new ReverseOrder()));
        Assert.Equal(new[] { "C", "B", "A" }, TestSessions.Titles(session.Queue));

        invoker.Undo();
        Assert.Same(original, session.Order);
        Assert.Equal(new[] { "A", "B", "C" }, TestSessions.Titles(session.Queue));
    }

    [Fact]
    public void Undo_Reverses_The_Most_Recent_Command_First()
    {
        IPlaybackSession session = TestSessions.ThreeTracks(new RecordingAudioOutput(100));
        CommandInvoker invoker = new CommandInvoker();
        invoker.Execute(new SetVolumeCommand(session, 30));
        invoker.Execute(new SetVolumeCommand(session, 60));

        invoker.Undo();
        Assert.Equal(30, session.Volume);

        invoker.Undo();
        Assert.Equal(100, session.Volume);

        Assert.False(invoker.Undo());             // nothing left: safe, returns false
    }

    [Fact]
    public void Rejected_Commands_Are_Logged_But_Not_Put_On_The_Undo_Stack()
    {
        IPlaybackSession session = TestSessions.ThreeTracks(new RecordingAudioOutput());
        CommandInvoker invoker = new CommandInvoker();

        Assert.False(invoker.Execute(new PauseCommand(session)));   // the State rejects it
        Assert.True(invoker.Execute(new PlayCommand(session)));

        Assert.Equal(new[] { "pause (rejected)", "play" }, invoker.History);
        Assert.False(invoker.CanUndo);            // transport commands are not undoable
    }

    [Fact]
    public void Macro_Command_Runs_All_Steps_And_One_Undo_Reverses_All_Of_Them()
    {
        IPlaybackSession session = TestSessions.ThreeTracks(new RecordingAudioOutput(50));
        IPlaybackOrder original = session.Order;
        CommandInvoker invoker = new CommandInvoker();

        invoker.Execute(new MacroCommand("party mode",
            new ChangeOrderCommand(session, new ReverseOrder()),
            new SetVolumeCommand(session, 100)));
        Assert.Equal(100, session.Volume);
        Assert.Equal("reverse", session.Order.Name);

        invoker.Undo();
        Assert.Equal(50, session.Volume);
        Assert.Same(original, session.Order);
    }

    [Fact]
    public void A_New_Command_Works_With_The_Invoker_Without_Changing_It()
    {
        IPlaybackSession session = TestSessions.ThreeTracks(new RecordingAudioOutput(80));
        CommandInvoker invoker = new CommandInvoker();

        invoker.Execute(new MuteCommand(session));
        Assert.Equal(0, session.Volume);

        invoker.Undo();
        Assert.Equal(80, session.Volume);
    }

    // Test-only command: shows the invoker is open for extension (no existing class edited).
    private sealed class MuteCommand : IUndoableCommand
    {
        private readonly IPlaybackSession _session;
        private int _previous;

        public MuteCommand(IPlaybackSession session) => _session = session;

        public string Description => "mute";

        public bool Execute()
        {
            _previous = _session.Volume;
            _session.SetVolume(0);
            return true;
        }

        public void Undo() => _session.SetVolume(_previous);
    }
}
