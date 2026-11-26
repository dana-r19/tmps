# Music Player: Structural Design Patterns (Lab 2, TMPS)

A console music player in C# that applies three structural design patterns, **Composite**, **Decorator** and **Facade**, on top of the creational patterns from Lab 1 (**Singleton**, **Factory Method**, **Builder**). Full explanation, UML diagrams and tests are in [REPORT.md](REPORT.md).

No sound is produced: "playing" prints what would be played, with the volume and fade-in in effect. This is enough for the lab (console app) and means no libraries are needed.

## Requirements

- [.NET SDK](https://dotnet.microsoft.com/download) 8.0 or newer. Check with `dotnet --version`.
- Internet on the first build only (to restore the xUnit test packages).

The projects target `net8.0` and are set to run on newer runtimes too (`RollForward=Major`). If the build complains about the target framework, change `<TargetFramework>` to your SDK's version (for example `net10.0`) in both `.csproj` files.

## Build and run

From the repository root:

```
dotnet build src/MusicPlayer
dotnet run --project src/MusicPlayer
```

## Play real songs (optional, Windows)

Put `.mp3` or `.wav` files in the `songs/` folder (named `Artist - Title.ext`), then:

```
dotnet run --project src/MusicPlayer -- --play
dotnet run --project src/MusicPlayer -- --play "D:\some\other\folder"
```

Every song is faded in and added to one playlist. Press **S** to skip a track and **Q** to quit. Sound goes through `SpeakerAudioOutput`, a second `IAudioOutput` that uses the [NAudio](https://github.com/naudio/NAudio) library; the patterns themselves are unchanged. Without `--play`, the console demo above runs as before and needs no sound device.

## Run the unit tests (one command)

```
dotnet test tests/MusicPlayer.Tests
```

## Project structure

```
src/MusicPlayer/
├── Program.cs                      CLIENT: uses only IMusicPlayer (the facade)
├── Domain/
│   ├── Models/                     Song (+ Mp3Song, WavSong, OggSong), TimeFormat
│   └── Audio/                      IAudioOutput, ConsoleAudioOutput
├── Creational/                     Lab 1 patterns
│   ├── PlayerSettings.cs           Singleton
│   ├── SongLoader.cs               Factory Method (+ registry)
│   └── PlaylistBuilder.cs          Builder
└── Structural/                     Lab 2 patterns
    ├── Composite/                  IPlayable, Track (leaf), Playlist (group)
    ├── Decorator/                  PlayableDecorator, Volume/FadeIn/Loop decorators
    └── Facade/                     IMusicPlayer, MusicPlayerFacade
tests/MusicPlayer.Tests/            xUnit tests + RecordingAudioOutput (test stub)
```

## How the patterns work together

```
Program (client)
   -> IMusicPlayer / MusicPlayerFacade              Facade
        -> SongLoaderRegistry -> Mp3SongLoader ...  Factory Method (Lab 1)
        -> PlaylistBuilder                          Builder (Lab 1)
        -> Playlist / Track                         Composite
        -> VolumeDecorator / FadeInDecorator / ...  Decorator
        -> IAudioOutput                             console or test stub
```

`PlayerSettings` (Singleton) supplies the default fade-in length and loop count.

## Extending it (no existing class is edited)

- **New decorator** (for example mute): add a class that extends `PlayableDecorator`.
- **New audio format:** add a `Song` subclass and a `SongLoader` subclass, then call `Register(".ext", ...)`.
- **New output** (for example a log file): add a class that implements `IAudioOutput`.
