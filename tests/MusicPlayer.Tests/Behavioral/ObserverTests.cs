using MusicPlayer.Behavioral.Observer;
using MusicPlayer.Behavioral.State;

namespace MusicPlayer.Tests.Behavioral;

public class ObserverTests
{
    [Fact]
    public void All_Subscribed_Observers_Are_Notified()
    {
        PlaybackSession session = TestSessions.ThreeTracks(new RecordingAudioOutput());
        RecordingObserver first = new RecordingObserver();
        RecordingObserver second = new RecordingObserver();
        session.Subscribe(first);
        session.Subscribe(second);

        session.Play();

        Assert.Equal(new[] { PlaybackEventKind.StateChanged, PlaybackEventKind.TrackStarted }, first.Kinds);
        Assert.Equal(first.Events, second.Events);
    }

    [Fact]
    public void An_Unsubscribed_Observer_Is_Not_Notified()
    {
        PlaybackSession session = TestSessions.ThreeTracks(new RecordingAudioOutput());
        RecordingObserver stays = new RecordingObserver();
        RecordingObserver leaves = new RecordingObserver();
        session.Subscribe(stays);
        session.Subscribe(leaves);

        session.Play();
        session.Unsubscribe(leaves);
        session.Pause();

        Assert.Equal(3, stays.Events.Count);
        Assert.Equal(2, leaves.Events.Count);     // nothing after unsubscribing
    }

    [Fact]
    public void Subscribing_The_Same_Observer_Twice_Notifies_It_Once()
    {
        PlaybackSession session = TestSessions.ThreeTracks(new RecordingAudioOutput());
        RecordingObserver observer = new RecordingObserver();
        session.Subscribe(observer);
        session.Subscribe(observer);

        session.SetVolume(40);

        Assert.Single(observer.Events);
    }

    [Fact]
    public void An_Observer_Can_Unsubscribe_Itself_While_Being_Notified()
    {
        PlaybackSession session = TestSessions.ThreeTracks(new RecordingAudioOutput());
        OneShotObserver oneShot = new OneShotObserver(session);
        RecordingObserver other = new RecordingObserver();
        session.Subscribe(oneShot);
        session.Subscribe(other);

        session.Play();                           // two events

        Assert.Equal(1, oneShot.Calls);
        Assert.Equal(2, other.Events.Count);      // the loop was not disturbed
    }

    [Fact]
    public void Events_Carry_The_State_Track_And_Volume_At_That_Moment()
    {
        PlaybackSession session = TestSessions.ThreeTracks(new RecordingAudioOutput(60));
        RecordingObserver observer = new RecordingObserver();
        session.Subscribe(observer);

        session.Play();

        PlaybackEvent started = observer.Events.Single(e => e.Kind == PlaybackEventKind.TrackStarted);
        Assert.Equal("Playing", started.State);
        Assert.Equal("A - Tester [MP3]", started.Track?.Name);
        Assert.Equal(60, started.Volume);
    }

    [Fact]
    public void Concrete_Observers_React_Only_To_Their_Events()
    {
        PlaybackSession session = TestSessions.ThreeTracks(new RecordingAudioOutput());
        ListeningStats stats = new ListeningStats();
        LoudVolumeWarning warning = new LoudVolumeWarning(85, TextWriter.Null);
        session.Subscribe(stats);
        session.Subscribe(warning);

        session.Play();                           // A, 10 s
        session.Next();                           // B, 30 s
        session.SetVolume(50);                    // not loud
        session.SetVolume(90);                    // loud

        Assert.Equal(2, stats.ItemsStarted);
        Assert.Equal(40, stats.SecondsQueued);
        Assert.Equal(1, warning.WarningCount);
    }

    private sealed class OneShotObserver : IPlaybackObserver
    {
        private readonly IPlaybackSubject _subject;

        public OneShotObserver(IPlaybackSubject subject) => _subject = subject;

        public int Calls { get; private set; }

        public void Update(PlaybackEvent playbackEvent)
        {
            Calls++;
            _subject.Unsubscribe(this);
        }
    }
}
