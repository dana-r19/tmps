using MusicPlayer.Creational;
using MusicPlayer.Domain.Audio;
using MusicPlayer.Domain.Models;
using MusicPlayer.Structural.Composite;
using MusicPlayer.Structural.Decorator;

namespace MusicPlayer.Structural.Facade;

/// <summary>
/// FACADE - each method is one or two delegating calls into the subsystem
/// (Factory Method, Builder, Composite, Decorator, audio output). The facade itself only
/// keeps a name-to-item library.
/// The facade adds no new features of its own: it only hides HOW the subsystem classes
/// are created and connected. The comments on each method show which pattern it calls.
/// </summary>
public sealed class MusicPlayerFacade : IMusicPlayer
{
    // Subsystem parts are injected, so the facade can be used with a console output,
    // real speakers, or a recording output in the unit tests.
    private readonly IAudioOutput _output;
    private readonly SongLoaderRegistry _loaders;
    private readonly Dictionary<string, IPlayable> _items = new Dictionary<string, IPlayable>();
    private readonly Dictionary<string, Playlist> _playlists = new Dictionary<string, Playlist>();

    public MusicPlayerFacade(IAudioOutput output, SongLoaderRegistry loaders)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _loaders = loaders ?? throw new ArgumentNullException(nameof(loaders));
    }

    public IReadOnlyCollection<string> Library => _items.Keys;

    public string ImportSong(string fileName, int durationSeconds)
    {
        Song song = _loaders.Load(fileName, durationSeconds);      // Factory Method
        return Store(new Track(song));                              // Composite leaf
    }

    public string ImportFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Audio file not found.", filePath);
        }

        _loaders.Load(filePath, 0);   // fail fast on an unsupported format, before opening the file
        return ImportSong(filePath, SpeakerAudioOutput.GetDurationSeconds(filePath));
    }

    public string CreatePlaylist(string playlistName, params string[] itemNames)
    {
        PlaylistBuilder builder = new PlaylistBuilder(_loaders).Named(playlistName);   // Builder
        foreach (string itemName in itemNames)
        {
            builder.Add(Find(itemName));
        }

        Playlist playlist = builder.Build();                        // Composite group
        _playlists[playlist.Name] = playlist;
        return Store(playlist);
    }

    public void AddToPlaylist(string playlistName, string itemName)
    {
        if (!_playlists.TryGetValue(playlistName, out Playlist? playlist))
        {
            throw new KeyNotFoundException($"No playlist named '{playlistName}'.");
        }

        playlist.Add(Find(itemName));
    }

    // The three With... methods hide Decorator construction: the client passes a name,
    // the facade looks up the item, wraps it, and stores the result under a new name.
    // Because the result is stored too, decorators can be stacked by calling these again.
    public string WithVolume(string itemName, int percent)
    {
        return Store(new VolumeDecorator(Find(itemName), percent));   // Decorator
    }

    public string WithFadeIn(string itemName, int? seconds = null)
    {
        int length = seconds ?? PlayerSettings.Instance.DefaultFadeInSeconds;    // Singleton
        return Store(new FadeInDecorator(Find(itemName), length));
    }

    public string WithLoop(string itemName, int? times = null)
    {
        int count = times ?? PlayerSettings.Instance.DefaultLoopCount;          // Singleton
        return Store(new LoopDecorator(Find(itemName), count));
    }

    // One call from the client starts the whole chain:
    // Facade -> Composite (playlist recursion) -> Decorators (effects) -> audio output.
    public void Play(string itemName)
    {
        Find(itemName).Play(_output);
    }

    // Internal library: maps the names the client uses to the real IPlayable objects,
    // so subsystem objects never leak out of the facade.
    private string Store(IPlayable item)
    {
        _items[item.Name] = item;
        return item.Name;
    }

    private IPlayable Find(string itemName)
    {
        if (!_items.TryGetValue(itemName, out IPlayable? item))
        {
            throw new KeyNotFoundException($"No item named '{itemName}' in the library.");
        }

        return item;
    }
}
