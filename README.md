# Music Player: Behavioral Design Patterns (Lab 3, TMPS)

A console music player in C#. Lab 3 adds four behavioral patterns, **State**, **Command**, **Observer** and **Strategy**, on top of the structural patterns from Lab 2 (**Composite**, **Decorator**, **Facade**) and the creational patterns from Lab 1 (**Singleton**, **Factory Method**, **Builder**).

The full explanation, UML diagrams and test logs are in [REPORT.md](REPORT.md). The Lab 2 report is kept in [docs/LAB2_REPORT.md](docs/LAB2_REPORT.md).

By default no sound is produced: "playing" prints what would be played, with the volume and fade-in that are in effect.

## Requirements

- [.NET SDK](https://dotnet.microsoft.com/download) 8.0 or newer. Check with `dotnet --version`.
- Internet on the first build only (to restore NuGet packages: xUnit, NAudio).

The projects target `net8.0` and also run on newer runtimes (`RollForward=Major`). If the build complains about the target framework, change `<TargetFramework>` to your SDK's version (for example `net10.0`) in both `.csproj` files.

## Build and run

From the repository root:

```
dotnet build src/MusicPlayer
dotnet run --project src/MusicPlayer                       # Lab 3 demo (scripted)
dotnet run --project src/MusicPlayer -- --interactive      # type the commands yourself
dotnet run --project src/MusicPlayer -- --structural       # Lab 2 demo
dotnet run --project src/MusicPlayer -- --play [folder]    # real sound, Windows (see below)
```

### Interactive mode

The player's behavior changes while it runs, driven by what you type:

| Command | Effect | Pattern |
|---|---|---|
| `play`, `pause`, `stop`, `next` | transport; the current state decides whether the action is allowed | Command → State |
| `vol 40` | set the volume (undoable) | Command |
| `order sequential\|reverse\|shuffle\|shortest` | switch the play order (undoable) | Command → Strategy |
| `party` | shuffle + volume 100 as one undoable macro | Command |
| `undo` | undo the last undoable command | Command |
| `watch stats`, `unwatch display`, `watch loud` | subscribe or unsubscribe an observer | Observer |
| `queue`, `history`, `stats`, `help`, `quit` | show information / exit | |

### Play real songs (optional, Windows)

Put `.mp3` or `.wav` files in the `songs/` folder (named `Artist - Title.ext`), then run with `--play` (or `--play "D:\some\folder"`). Press **S** to skip a track and **Q** to quit.

## Run the unit tests (one command)

```
dotnet test tests/MusicPlayer.Tests
```

There are 49 tests: 27 for Lab 3 (State, Command, Observer, Strategy and their interaction) and 22 for Labs 1 and 2.

## Project structure

```
src/MusicPlayer/
├── Program.cs                 picks which client to run from the command-line arguments
├── Client/                    CLIENT CODE
│   ├── BehavioralDemo.cs      Lab 3 scripted demo
│   ├── InteractiveShell.cs    user-driven mode (behavior changes at runtime)
│   ├── DemoLibrary.cs         fills the library through the facade
│   ├── StructuralDemo.cs      Lab 2 demo
│   └── FolderPlayer.cs        real playback from a folder
├── Domain/                    DOMAIN MODELS: Song (+ Mp3/Wav/Ogg), TimeFormat, IAudioOutput + outputs
├── Creational/                Lab 1: PlayerSettings (Singleton), SongLoader (Factory Method), PlaylistBuilder (Builder)
├── Structural/                Lab 2: Composite/, Decorator/, Facade/
└── Behavioral/                Lab 3
    ├── State/                 IPlaybackSession, PlaybackSession (context), IPlayerState, PlayerStateBase,
    │                          StoppedState, PlayingState, PausedState
    ├── Command/               ICommand, IUndoableCommand, Play/Pause/Stop/Next commands,
    │                          SetVolumeCommand, ChangeOrderCommand, MacroCommand, CommandInvoker
    ├── Observer/              IPlaybackSubject, IPlaybackObserver, PlaybackEvent,
    │                          NowPlayingDisplay, ListeningStats, LoudVolumeWarning
    └── Strategy/              IPlaybackOrder, Sequential/Reverse/Shuffle/ShortestFirst orders, PlayQueue (context)
tests/MusicPlayer.Tests/
├── Behavioral/                StateTests, CommandTests, ObserverTests, StrategyTests, InteractionTests
└── *.cs                       Lab 1/2 tests and shared test stubs
```

## How the patterns work together

```
Client (demo / interactive shell)
  -> IMusicPlayer.OpenSession(...)                Facade (Lab 2) builds the queue from library items
  -> CommandInvoker.Execute(ICommand)             Command: logged, undoable if IUndoableCommand
       -> IPlaybackSession (PlaybackSession)      receiver, State context, Observer subject
            -> IPlayerState                       State: decides if the action is allowed, and the next state
            -> PlayQueue -> IPlaybackOrder        Strategy: which item comes next
            -> IPlayable.Play(IAudioOutput)       Composite + Decorator (Lab 2) play the item
            -> IPlaybackObserver.Update(event)    Observer: display, stats, loud-volume warning
```

## Extending it (new classes only)

| To add | Do this | Existing classes edited |
|---|---|---|
| A new command (e.g. mute) | class implementing `ICommand` or `IUndoableCommand` | none (`CommandInvoker` is unchanged); one line in `InteractiveShell`'s table to bind it to a word |
| A new play order (e.g. by artist) | class implementing `IPlaybackOrder` | none; one line in the shell's order table |
| A new observer (e.g. scrobbler) | class implementing `IPlaybackObserver`, then `session.Subscribe(...)` | none |
| A new state (e.g. Buffering) | class extending `PlayerStateBase`, overriding only its allowed actions | only the states that transition *into* it (inherent to State) |
| A new effect (Lab 2) | class extending `PlayableDecorator` | none |
| A new audio format (Lab 1) | `Song` + `SongLoader` subclasses, `Register(".ext", ...)` | none |
