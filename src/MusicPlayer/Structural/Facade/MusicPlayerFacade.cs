using MusicPlayer.Behavioral.State;
using MusicPlayer.Behavioral.Strategy;
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
/// </summary>
public sealed class MusicPlayerFacade : IMusicPlayer
{
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

    public string WithVolume(string itemName, int percent)
    {
        return Store(new VolumeDecorator(Find(itemName), percent));
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

    public void Play(string itemName)
    {
        Find(itemName).Play(_output);
    }

    public IPlaybackSession OpenSession(params string[] itemNames)
    {
        IPlayable[] items = itemNames.Select(Find).ToArray();
        return new PlaybackSession(_output, items, new SequentialOrder());   // State + Strategy (Lab 3)
    }

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
