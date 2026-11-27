using MusicPlayer.Behavioral.Command;
using MusicPlayer.Behavioral.Observer;
using MusicPlayer.Behavioral.State;
using MusicPlayer.Behavioral.Strategy;
using MusicPlayer.Creational;
using MusicPlayer.Structural.Facade;

namespace MusicPlayer.Tests.Behavioral;

/// <summary>
/// One execution path through several patterns:
/// Command (invoker) -> State (session) -> Observer (notified) and Composite/Decorator (played),
/// with the session opened through the Lab 2 Facade.
/// </summary>
public class InteractionTests
{
    private static (IPlaybackSession Session, RecordingAudioOutput Output) OpenSession()
    {
        RecordingAudioOutput output = new RecordingAudioOutput(100);
        IMusicPlayer player = new MusicPlayerFacade(output, SongLoaderRegistry.CreateDefault());
        string one = player.ImportSong("Band - One.mp3", 10);
        string two = player.ImportSong("Band - Two.wav", 20);
        string quietOne = player.WithVolume(one, 40);
        string mix = player.CreatePlaylist("Mix", two, player.WithFadeIn(one, 2));
        return (player.OpenSession(quietOne, mix), output);
    }

    [Fact]
    public void A_Command_Changes_The_State_Observers_Hear_About_It_And_The_Decorated_Item_Plays()
    {
        var (session, output) = OpenSession();
        RecordingObserver observer = new RecordingObserver();
        session.Subscribe(observer);

        new CommandInvoker().Execute(new PlayCommand(session));

        Assert.Equal("Playing", session.StateName);                               // State
        Assert.Equal("Stopped -> Playing", observer.Events[0].Message);           // Observer
        Assert.Equal("render:One - Band [MP3]|10s|vol 40|fade 0", Assert.Single(output.Renders));  // Decorator
        Assert.Equal(100, output.Volume);                                         // decorator restored it
    }

    [Fact]
    public void Next_Plays_A_Whole_Playlist_As_One_Queue_Item()
    {
        var (session, output) = OpenSession();
        CommandInvoker invoker = new CommandInvoker();
        invoker.Execute(new PlayCommand(session));

        invoker.Execute(new NextCommand(session));

        Assert.Equal(new[]
        {
            "render:One - Band [MP3]|10s|vol 40|fade 0",
            "announce:Playlist 'Mix': 2 track(s), 0:30",
            "render:Two - Band [WAV]|20s|vol 100|fade 0",
            "render:One - Band [MP3]|10s|vol 100|fade 2",
        }, output.Events);                                                        // Composite
    }

    [Fact]
    public void Undoing_A_Strategy_Change_Is_Announced_To_Observers()
    {
        var (session, _) = OpenSession();
        RecordingObserver observer = new RecordingObserver();
        session.Subscribe(observer);
        CommandInvoker invoker = new CommandInvoker();

        invoker.Execute(new ChangeOrderCommand(session, new ReverseOrder()));
        invoker.Undo();

        Assert.Equal(new[] { PlaybackEventKind.OrderChanged, PlaybackEventKind.OrderChanged }, observer.Kinds);
        Assert.Equal("sequential", session.Order.Name);
        Assert.Equal(new[] { "order reverse", "undo order reverse" }, invoker.History);
    }
}
