namespace MusicPlayer.Structural.Facade;

// =====================================================================================
// STRUCTURAL PATTERN: FACADE
// -------------------------------------------------------------------------------------
// What it is:
//   Facade gives one simple, high-level interface to a complex subsystem made of many
//   classes. The client talks only to the facade; the facade knows which subsystem
//   classes to call and in what order.
//
// How it is used here:
//   - IMusicPlayer       = Facade interface (this file)
//   - MusicPlayerFacade  = Facade implementation
//   - Subsystem          = SongLoaderRegistry (Factory Method), PlaylistBuilder (Builder),
//                          PlayerSettings (Singleton), Track/Playlist (Composite),
//                          Volume/FadeIn/Loop decorators (Decorator), IAudioOutput
//   - Client             = Program.cs
//
// Why:
//   Without the facade, Program.cs would have to create loaders, wrap songs in Tracks,
//   run the builder, construct decorators and pass the audio output around. With it,
//   the client just writes player.ImportSong(...), player.WithVolume(...), player.Play(...)
//   using plain names (strings). The subsystem can change without changing the client.
// =====================================================================================

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
}
