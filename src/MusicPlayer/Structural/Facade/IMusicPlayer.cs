using MusicPlayer.Behavioral.State;

namespace MusicPlayer.Structural.Facade;

/// <summary>
/// FACADE - the simple interface the client uses. Everything is addressed by name, so the
/// client never sees loaders, builders, playlists or decorators.
/// </summary>
public interface IMusicPlayer
{
    /// <summary>Names of every item in the library (songs, decorated items, playlists).</summary>
    IReadOnlyCollection<string> Library { get; }

    string ImportSong(string fileName, int durationSeconds);

    /// <summary>Imports a real audio file; its length is read from the file.</summary>
    string ImportFile(string filePath);

    string CreatePlaylist(string playlistName, params string[] itemNames);
    void AddToPlaylist(string playlistName, string itemName);
    string WithVolume(string itemName, int percent);
    string WithFadeIn(string itemName, int? seconds = null);
    string WithLoop(string itemName, int? times = null);
    void Play(string itemName);

    /// <summary>
    /// Lab 3: queues the named items (each can be a song, a decorated item or a whole playlist)
    /// in a playback session that is controlled step by step (play, pause, next, ...).
    /// </summary>
    IPlaybackSession OpenSession(params string[] itemNames);
}
