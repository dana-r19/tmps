using MusicPlayer.Domain.Models;
using MusicPlayer.Structural.Composite;

namespace MusicPlayer.Tests;

internal static class TestSongs
{
    public static Track Track(string title, int seconds)
    {
        return new Track(new Mp3Song(title, "Tester", seconds));
    }
}
