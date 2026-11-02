# Creational Design Patterns in a Python Music Player

## Course: TMPS (Software Design Techniques and Mechanisms), Laboratory Work 1
## Topic: Creational Design Patterns
## Author: Romanov Dana
## Group: FAF-241

---

## Objectives

1. Study and understand the creational design patterns.
2. Choose a domain, define its main classes and choose appropriate instantiation mechanisms.
3. Use creational design patterns for object instantiation in a sample project.

## Some Theory

Creational design patterns deal with **how objects are created**. Creating objects directly with `new`/`Class()` everywhere couples the code to concrete classes and spreads creation details around the program. Creational patterns optimize, hide or control the creation process.

| Pattern | One-line idea | Used here? |
|---|---|---|
| **Singleton** | Exactly one instance, with global access to it | **Yes** (`Settings`) |
| **Builder** | Construct a complex object step by step | **Yes** (`PlaylistBuilder` + `PlaylistDirector`) |
| **Factory Method** | A method creates the object; subclasses decide which class | **Yes** (`SongLoader` and its subclasses) |
| **Prototype** | Create objects by cloning an existing one | **Yes, as a bonus** (`Playlist.clone`) |
| **Abstract Factory** | An interface for creating *families* of related objects | Theory only (Section 7) |
| **Object Pool** | Reuse expensive objects instead of recreating them | Theory only (Section 7) |

## Domain

A command-line **music player**. The program scans a folder of audio files, builds a playlist and plays it, with pause, resume and skip controls. The project continues the SOLID lab (same domain), so the SRP/OCP/DIP structure from that lab is kept.

Main entities:

| Entity | Role |
|---|---|
| `Song` (+ `Mp3Song`, `WavSong`, `OggSong`, `M4aSong`) | A track; one subclass per audio format |
| `Playlist` | A named list of songs |
| `Settings` | Global configuration (songs folder, preview length, backend) |
| `AudioBackend` (`PygameBackend`, `SimulatedBackend`) | Produces sound (or simulates it) |
| `MusicPlayer` | Plays a playlist and handles pause/resume/skip/stop |

## Project Structure

```
lab1_creational/
├── client/
│   ├── main.py              Entry point (the "client" that uses the domain)
│   └── demo_patterns.py     Shows every pattern in action, no sound
├── domain/
│   ├── models/
│   │   ├── song.py          Song + format subclasses
│   │   └── playlist.py      Playlist (+ clone = Prototype)
│   ├── factory/
│   │   ├── song_factory.py  Factory Method: SongLoader hierarchy
│   │   └── playlist_builder.py  Builder + Director
│   ├── playback/
│   │   ├── backends.py      AudioBackend abstraction + implementations
│   │   └── player.py        MusicPlayer (play/pause/resume/skip/stop)
│   ├── ordering.py          PlaybackOrder strategies (from the SOLID lab)
│   └── settings.py          Singleton
├── songs/                   Audio files (4 generated demo tunes included)
├── make_demo_songs.py       Regenerates the demo tunes
└── requirements.txt         pygame (optional, only for real sound)
```

This follows the required split: **client**, **domain/factory**, **domain/models**.

---

## Implementation

### 1. Singleton: `domain/settings.py`

**Problem.** Several parts of the program need the same configuration. If each part created its own `Settings`, a change made in one place would not be seen elsewhere.

**Solution.** `Settings()` always returns the *same* object. The first call creates it; every later call returns the stored instance.

```python
class Settings:
    _instance = None
    _lock = threading.Lock()

    def __new__(cls):
        if cls._instance is None:
            with cls._lock:
                if cls._instance is None:        # checked again inside the lock
                    instance = super().__new__(cls)
                    instance.songs_folder = "songs"
                    instance.preview_seconds = 20
                    instance.backend = "auto"
                    instance.quick_mix_size = 5
                    cls._instance = instance
        return cls._instance
```

* `__new__` (not `__init__`) is overridden because `__new__` is what *creates* the object.
* The lock plus the second `if` ("double-checked locking") keeps it safe if two threads call `Settings()` at the same time.
* Proof in `demo_patterns.py`: `Settings() is Settings()` is `True`, and a change through one reference is visible through the other.

**Trade-offs (likely quiz topic).** A Singleton is global state: it hides dependencies, makes unit tests harder (state leaks between tests) and can conflict with Dependency Inversion. In this project it is read in the client and in `PlaylistDirector.quick_mix` only.

### 2. Factory Method: `domain/factory/song_factory.py`

**Problem.** Different audio formats need different handling at creation time (for example, a `.wav` file contains its length in its header, an `.mp3` does not). Putting `if extension == ".wav": ... elif ...` where songs are created would force edits in many places whenever a format is added.

**Solution.** The base class `SongLoader` (the *Creator*) contains the shared logic and calls an abstract **factory method**, `create_song`. Each subclass decides which concrete `Song` to create.

```python
class SongLoader(ABC):                      # Creator
    def load(self, path) -> Song:           # shared logic
        path = Path(path)
        artist, sep, title = path.stem.partition(" - ")
        if not sep:
            artist, title = "Unknown", path.stem
        return self.create_song(title.strip(), artist.strip(), path)

    @abstractmethod
    def create_song(self, title, artist, path) -> Song:   # the factory method
        ...

class WavSongLoader(SongLoader):            # Concrete Creator
    def create_song(self, title, artist, path):
        with wave.open(str(path)) as w:
            duration = int(w.getnframes() / w.getframerate())
        return WavSong(title, artist, duration, str(path))    # Concrete Product

class Mp3SongLoader(SongLoader):
    def create_song(self, title, artist, path):
        return Mp3Song(title, artist, 0, str(path))
```

| Role in the pattern | Class |
|---|---|
| Product (abstraction) | `Song` |
| Concrete products | `Mp3Song`, `WavSong`, `OggSong`, `M4aSong` |
| Creator | `SongLoader` |
| Concrete creators | `Mp3SongLoader`, `WavSongLoader`, `OggSongLoader`, `M4aSongLoader` |

Adding a format means one new product class, one new loader class and one line in the `_LOADERS` dictionary. No existing loader is edited (this is also the Open/Closed Principle). The small `loader_for(path)` function that picks a loader from the file extension is just a lookup helper; it is not part of the pattern itself.

### 3. Builder (with Director): `domain/factory/playlist_builder.py`

**Problem.** A playlist has several optional parts: a name, songs (added one by one or from a folder), an order and a size limit. A constructor with many parameters would be unreadable and inflexible, and the steps must happen in a sensible sequence (for example, order *after* adding songs).

**Solution.** `PlaylistBuilder` assembles the playlist step by step. Each step returns `self`, so the calls can be chained, and `build()` returns the finished `Playlist`.

```python
playlist = (PlaylistBuilder()
            .named("Study")
            .add_folder("songs")          # uses the Factory Method internally
            .ordered_by(ByTitle())
            .limited_to(2)
            .build())
```

```python
def build(self) -> Playlist:
    songs = self._order.arrange(self._songs)
    if self._limit:
        songs = songs[: self._limit]
    playlist = Playlist(self._name, songs)
    self.reset()                          # the builder can be reused
    return playlist
```

The **Director** is the optional second half of the pattern. `PlaylistDirector` stores ready-made recipes that call the builder's steps in a fixed order:

```python
def quick_mix(self, folder, size=None) -> Playlist:
    size = size or Settings().quick_mix_size
    return (self._builder.named("Quick mix").add_folder(folder)
            .ordered_by(Shuffle()).limited_to(size).build())
```

| Role | Class |
|---|---|
| Builder | `PlaylistBuilder` |
| Product | `Playlist` |
| Director | `PlaylistDirector` (`quick_mix`, `alphabetical`) |

Try it: `py client/main.py --preset quickmix --list`.

### 4. Prototype (bonus): `domain/models/playlist.py`

**Problem.** Sometimes a new object should start as a copy of an existing one.

**Solution.** `Playlist.clone()` creates an independent deep copy, so editing the clone does not change the original.

```python
def clone(self, new_name=None) -> "Playlist":
    duplicate = copy.deepcopy(self)
    if new_name:
        duplicate.name = new_name
    return duplicate
```

*Shallow vs deep copy:* a shallow copy would share the same `songs` list between both playlists; `deepcopy` duplicates it. The demo prints `independent lists: True`.

### Pause, resume and skip: `domain/playback/player.py`

`MusicPlayer` was extended with `pause()`, `resume()`, `toggle()`, `skip()` and `stop()`. The audio side was moved behind the `AudioBackend` abstraction (`start / pause / resume / stop / has_finished`), so the player works with real sound (`PygameBackend`) or with a clock-only `SimulatedBackend` that needs no libraries. Time spent paused is not counted toward a song's length or the preview limit.

In `client/main.py` a background thread reads commands from the keyboard: `p` (pause/resume), `s` (skip), `q` (quit), each followed by Enter.

---

## How the SOLID lab still applies

| Principle | Where it still shows |
|---|---|
| SRP | `Song` = data, `Playlist` = data, loaders = creating songs, builder = assembling playlists, backend = sound, player = coordination |
| OCP | New formats, orders and backends are added as new classes |
| DIP | `MusicPlayer` depends on the `AudioBackend` abstraction, injected through the constructor |

## How to Run

```
py make_demo_songs.py                       # only if songs/ is empty
py client/demo_patterns.py                  # every pattern, no sound
py client/main.py --list                    # show the playlist
py client/main.py                           # play (real sound)
py client/main.py --order shuffle
py client/main.py --preset quickmix --limit 3
py client/main.py --backend simulated       # no sound, no libraries
py client/main.py --preview 0               # full songs (default: 20 s each)
```

The project itself uses only Python's standard library. `pygame` is optional and used only for real sound; without it the player falls back to the simulated backend. (If your teacher's "no libraries" rule is strict, run with `--backend simulated` or use the demo script.)

To use my own music, I copy `.mp3`, `.wav`, `.ogg` or `.m4a` files into `songs/` and name them `Artist - Title.ext`.

## Verification

| Check | Method | Result |
|---|---|---|
| Singleton | `demo_patterns.py` | `Settings() is Settings()` is `True`; change visible through both |
| Factory Method | `demo_patterns.py` with one real and three stand-in files | Each extension produced the matching loader and `Song` subclass |
| Builder / Director | `demo_patterns.py`, `--list`, `--preset quickmix` | Correct order and limit; the builder is reusable |
| Prototype | `demo_patterns.py` | Clone modified, original unchanged |
| Pause / resume / skip | Timed test with the simulated backend | Pause time excluded; skipping starts the next song; quit stops |
| `PygameBackend` | Checked against a stand-in for pygame | Paused state is not reported as "finished" |

---

### Abstract Factory
An interface for creating **families** of related objects without naming their concrete classes. Example for this domain (not in the code):

```python
class OutputFamily(ABC):
    @abstractmethod
    def create_backend(self) -> AudioBackend: ...
    @abstractmethod
    def create_display(self) -> Display: ...

class RealFamily(OutputFamily):       # PygameBackend + GuiDisplay
    ...
class TestFamily(OutputFamily):       # SimulatedBackend + ConsoleDisplay
    ...
```
The client picks one factory and automatically gets objects that belong together.

### Object Pool
Keeps a set of reusable, expensive-to-create objects; clients **acquire** one and **release** it back instead of creating and destroying it. Typical uses: database connections, threads, audio channels.

```python
class ChannelPool:
    def __init__(self, size): self._free = [AudioChannel() for _ in range(size)]
    def acquire(self): return self._free.pop()
    def release(self, channel): self._free.append(channel)
```

### Quick comparison

| Question | Answer |
|---|---|
| Factory Method vs Abstract Factory | Factory Method: **one** product, uses **inheritance** (a subclass overrides one method). Abstract Factory: a **family** of products, uses **composition** (a factory object with several creation methods). |
| Builder vs Factory | A factory returns a finished object in one call. A builder constructs a complex object over several steps and can produce different representations. |
| Builder vs Abstract Factory | Builder focuses on step-by-step construction of one complex object; Abstract Factory returns families of simple-to-moderate objects immediately. |
| Prototype vs Factory Method | Prototype copies an existing instance (no subclass per product); Factory Method creates a new instance through a subclass. |
| Singleton problems | Global state, hidden dependencies, hard to test, needs care with threads. |
| When Object Pool? | When creation is costly and objects are used briefly and often. |
| Shallow vs deep copy | Shallow shares nested objects; deep duplicates them. Prototype normally needs a deep copy. |
| Why creational patterns at all? | They decouple code from concrete classes and centralize creation rules. |

---

## Conclusions

Four creational patterns were applied to a working music player: **Singleton** (shared configuration), **Factory Method** (format-specific song creation), **Builder with Director** (step-by-step playlist assembly, plus ready-made recipes) and **Prototype** (playlist cloning). Each solved a concrete creation problem in the code rather than being added for its own sake. Combined with the SOLID structure from the previous lab, adding a new format, order or audio backend still means adding a class rather than editing working code. The main lesson about trade-offs is Singleton: it is convenient, but it adds global state, so it is kept to configuration only.


