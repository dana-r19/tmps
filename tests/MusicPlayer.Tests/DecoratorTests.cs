using MusicPlayer.Structural.Composite;
using MusicPlayer.Structural.Decorator;

namespace MusicPlayer.Tests;

public class DecoratorTests
{
    [Fact]
    public void Decorators_Can_Be_Stacked_In_Any_Order_With_The_Same_Result()
    {
        IPlayable loopThenVolume = new LoopDecorator(new VolumeDecorator(TestSongs.Track("A", 10), 80), 3);
        IPlayable volumeThenLoop = new VolumeDecorator(new LoopDecorator(TestSongs.Track("A", 10), 3), 80);

        RecordingAudioOutput first = new RecordingAudioOutput();
        RecordingAudioOutput second = new RecordingAudioOutput();
        loopThenVolume.Play(first);
        volumeThenLoop.Play(second);

        Assert.Equal(30, loopThenVolume.DurationSeconds);
        Assert.Equal(30, volumeThenLoop.DurationSeconds);
        Assert.Equal(3, first.Renders.Count);
        Assert.All(first.Renders, e => Assert.Contains("vol 80", e));
        Assert.Equal(first.Events, second.Events);
    }

    [Fact]
    public void Volume_Decorator_Applies_Its_Volume_And_Restores_The_Previous_One()
    {
        RecordingAudioOutput output = new RecordingAudioOutput(100);

        new VolumeDecorator(TestSongs.Track("A", 10), 40).Play(output);

        Assert.Contains("vol 40", Assert.Single(output.Renders));
        Assert.Equal(100, output.Volume);
    }

    [Fact]
    public void FadeIn_Decorator_Applies_Fade_And_Restores_The_Previous_One()
    {
        RecordingAudioOutput output = new RecordingAudioOutput();

        new FadeInDecorator(TestSongs.Track("A", 10), 5).Play(output);

        Assert.Contains("fade 5", Assert.Single(output.Renders));
        Assert.Equal(0, output.FadeInSeconds);
    }

    [Fact]
    public void Loop_Decorator_Multiplies_Duration_And_Track_Count()
    {
        IPlayable looped = new LoopDecorator(TestSongs.Track("A", 10), 4);

        Assert.Equal(40, looped.DurationSeconds);
        Assert.Equal(4, looped.TrackCount);
    }

    [Fact]
    public void Decorating_Does_Not_Change_The_Wrapped_Object()
    {
        Track track = TestSongs.Track("A", 10);

        IPlayable decorated = new LoopDecorator(new VolumeDecorator(track, 50), 2);

        Assert.Equal("A - Tester [MP3]", track.Name);
        Assert.Equal(10, track.DurationSeconds);
        Assert.Equal(1, track.TrackCount);
        Assert.Equal("A - Tester [MP3] (vol 50%) (x2)", decorated.Name);
    }

    [Fact]
    public void Volume_Decorator_Rejects_Values_Outside_0_To_100()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new VolumeDecorator(TestSongs.Track("A", 10), 101));
    }
}
