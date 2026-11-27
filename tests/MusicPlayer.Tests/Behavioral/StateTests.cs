using MusicPlayer.Behavioral.State;
using MusicPlayer.Behavioral.Strategy;
using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Tests.Behavioral;

public class StateTests
{
    [Fact]
    public void Play_From_Stopped_Enters_Playing_And_Starts_The_First_Track()
    {
        RecordingAudioOutput output = new RecordingAudioOutput();
        PlaybackSession session = TestSessions.ThreeTracks(output);

        Assert.Equal("Stopped", session.StateName);
        Assert.True(session.Play());

        Assert.Equal("Playing", session.StateName);
        Assert.Contains("A - Tester", Assert.Single(output.Renders));
    }

    [Fact]
    public void Pause_And_Resume_Move_Between_Playing_And_Paused_Without_Restarting_The_Track()
    {
        RecordingAudioOutput output = new RecordingAudioOutput();
        PlaybackSession session = TestSessions.ThreeTracks(output);
        session.Play();

        Assert.True(session.Pause());
        Assert.Equal("Paused", session.StateName);

        Assert.True(session.Play());
        Assert.Equal("Playing", session.StateName);
        Assert.Single(output.Renders);            // resumed, not played a second time
    }

    [Fact]
    public void Forbidden_Transitions_Are_Rejected_And_Leave_The_State_Unchanged()
    {
        RecordingAudioOutput output = new RecordingAudioOutput();
        PlaybackSession session = TestSessions.ThreeTracks(output);

        // Stopped: only Play is allowed.
        Assert.False(session.Pause());
        Assert.False(session.Stop());
        Assert.False(session.Next());
        Assert.Equal("Stopped", session.StateName);

        // Playing: Play is rejected.
        session.Play();
        Assert.False(session.Play());
        Assert.Equal("Playing", session.StateName);

        // Paused: Pause is rejected.
        session.Pause();
        Assert.False(session.Pause());
        Assert.Equal("Paused", session.StateName);

        Assert.Single(output.Renders);            // nothing rejected produced sound
    }

    [Fact]
    public void Next_After_The_Last_Item_Stops_The_Player()
    {
        RecordingAudioOutput output = new RecordingAudioOutput();
        PlaybackSession session = TestSessions.ThreeTracks(output);
        session.Play();
        session.Next();
        session.Next();

        Assert.True(session.Next());

        Assert.Equal("Stopped", session.StateName);
        Assert.Equal(3, output.Renders.Count);
    }

    [Fact]
    public void Next_While_Paused_Starts_The_Following_Item()
    {
        RecordingAudioOutput output = new RecordingAudioOutput();
        PlaybackSession session = TestSessions.ThreeTracks(output);
        session.Play();
        session.Pause();

        Assert.True(session.Next());

        Assert.Equal("Playing", session.StateName);
        Assert.Contains("B - Tester", output.Renders[^1]);
    }

    [Fact]
    public void Stop_Then_Play_Starts_Again_From_The_First_Item()
    {
        RecordingAudioOutput output = new RecordingAudioOutput();
        PlaybackSession session = TestSessions.ThreeTracks(output);
        session.Play();
        session.Next();

        session.Stop();
        session.Play();

        Assert.Contains("A - Tester", output.Renders[^1]);
    }

    [Fact]
    public void Play_With_An_Empty_Queue_Is_Rejected()
    {
        PlaybackSession session = new PlaybackSession(
            new RecordingAudioOutput(), Array.Empty<IPlayable>(), new SequentialOrder());

        Assert.False(session.Play());
        Assert.Equal("Stopped", session.StateName);
    }
}
