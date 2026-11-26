namespace MusicPlayer.Domain.Models;

public static class TimeFormat
{
    /// <summary>235 seconds -> "3:55".</summary>
    public static string Format(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes}:{seconds:D2}";
    }
}
