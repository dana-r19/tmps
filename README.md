# Applying the SOLID Principles in a Python Music Player

**Course topic:** Software Design Techniques and Mechanisms
**Principles implemented:** S (Single Responsibility), O (Open/Closed), D (Dependency Inversion)
**Language:** Python 3.10+ (audio playback via `pygame`)
**Date:** October 2026

---

## Abstract

SOLID is a set of five object-oriented design principles whose goal is to make software easier to understand, maintain, test and extend. This report describes a small command-line music player written in Python that applies three of them: the **Single Responsibility Principle (SRP)**, the **Open/Closed Principle (OCP)** and the **Dependency Inversion Principle (DIP)**. The player scans a folder of audio files, arranges the songs in a chosen order and plays them. For each principle the report states the definition, shows the exact code that implements it, contrasts it with a naive design that would violate it, and explains what practical benefit the principle brought. The report also states the limits of the project honestly, including the two principles that were deliberately left out (Liskov Substitution and Interface Segregation).

---

## Table of Contents

1. Introduction
2. Theoretical Background
3. Project Overview and Architecture
4. Implementation of the Principles
   - 4.1 Single Responsibility Principle
   - 4.2 Open/Closed Principle
   - 4.3 Dependency Inversion Principle
5. How the Three Principles Work Together
6. Extending the Project (Worked Example)
7. Running and Verifying the Project
8. Limitations and Future Work
9. Conclusion
10. Glossary
11. References

---

## 1. Introduction

Programs rarely stay the size they start as. A first version of a music player might be fifty lines in one file: it reads songs, sorts them, prints them and plays them. When a new requirement arrives (for example "also shuffle" or "also write a log"), the quickest fix is to add another `if` branch to the same file. After a few such changes the code is tangled: every feature touches every other feature, and a small edit can break something unrelated.

The SOLID principles are guidelines for avoiding this outcome. This project is intentionally small so that the principles are easy to see, but it is a real, working program: it plays actual audio files from disk.

**Objectives**

1. Build a working music player in Python.
2. Apply SRP, OCP and DIP in a way that is visible in the code structure.
3. Document the reasoning so the design can be understood and defended, not just copied.

---

## 2. Theoretical Background

According to the course material, SOLID is an acronym for five OOP design principles whose intent is to make software more understandable, easier to maintain and test, and extendable. This project uses three of them.

| Letter | Principle | Core idea |
|---|---|---|
| **S** | Single Responsibility | A module, class or function is responsible for one part of the functionality, so it has only one reason to change. |
| **O** | Open/Closed | Software should be open for extension but closed for modification: behavior is extended without editing existing internals. |
| **L** | Liskov Substitution | *(not implemented)* Subclass instances must be usable wherever the parent class is expected without breaking the program. |
| **I** | Interface Segregation | *(not implemented)* A client should not be forced to depend on methods it does not use; large interfaces are split into smaller ones. |
| **D** | Dependency Inversion | High-level code should depend on abstractions, not on concrete implementations; modules at different levels communicate through abstractions. |

Two supporting concepts appear throughout the report:

- **Abstraction.** In Python this is an *abstract base class* (ABC) from the `abc` module: a class that declares *what* operations exist (`@abstractmethod`) without saying *how* they work. It cannot be instantiated directly.
- **Dependency injection.** Instead of a class creating the objects it needs, those objects are passed in from outside (here, through the constructor). This is the usual technique for achieving DIP.

---

## 3. Project Overview and Architecture

### 3.1 What the program does

1. Scans a folder (`songs/`) for `.mp3`, `.wav`, `.ogg` and, optionally, `.m4a` files.
2. Reads the artist and title from the file name, which must follow the pattern `Artist - Title.ext`.
3. Arranges the songs: in folder order, alphabetically by title, or shuffled.
4. Plays each song with `pygame` (default: the first 20 seconds, adjustable), or in `--silent` mode just lists them.

### 3.2 File structure

```
lab0/
├── main.py              Entry point; chooses the concrete classes
├── player.py            MusicPlayer: coordinates playback
├── models.py            Song: plain data
├── library.py           SongRepository (abstraction) + concrete repositories
├── ordering.py          PlaybackOrder (abstraction) + InOrder, ByTitle, Shuffle
├── output.py            AudioOutput (abstraction) + Console, LogFile, Pygame outputs
├── make_demo_songs.py   Generates three small demo tunes
├── requirements.txt     pygame
└── songs/               Audio files to play
```

### 3.3 Components and roles

| Component | Kind | Role |
|---|---|---|
| `Song` | Data class | Holds title, artist, duration and file path. |
| `SongRepository` | Abstraction | "Something that can give me a list of songs." |
| `InMemorySongRepository`, `FolderSongRepository` | Concrete | Two different ways to obtain songs. |
| `PlaybackOrder` | Abstraction | "Something that can arrange a list of songs." |
| `InOrder`, `ByTitle`, `Shuffle` | Concrete | Three different arrangements. |
| `AudioOutput` | Abstraction | "Something that can present one song." |
| `ConsoleOutput`, `LogFileOutput`, `PygameOutput` | Concrete | Print, log to file, or actually play sound. |
| `MusicPlayer` | Coordinator | Gets songs, orders them, sends each to the output. |

### 3.4 Dependency structure

```
                    main.py
          (decides which concrete classes to use)
                       |
                       v
                 +-------------+
                 | MusicPlayer |   <- high-level logic
                 +-------------+
                  |     |     |
   depends on     v     v     v     (abstractions only)
        SongRepository  PlaybackOrder  AudioOutput
              ^              ^              ^
              |              |              |
   InMemory..., Folder...  InOrder, ByTitle,  Console, LogFile,
   (concrete, low-level)   Shuffle            Pygame
```

The key observation: `MusicPlayer` points only at the three abstractions in the middle row. The concrete classes also point at those abstractions (they inherit from them). Nothing in `MusicPlayer` points at a concrete class. This is the "inversion" in Dependency Inversion.

---

## 4. Implementation of the Principles

### 4.1 Single Responsibility Principle (SRP)

**Definition.** A class should have one reason to change.

**Where it is applied.** Every class in the project has exactly one job:

| Class | Its single responsibility | It does *not* |
|---|---|---|
| `Song` | Describe a song | load, sort, print or play |
| `FolderSongRepository` | Find audio files and turn them into `Song` objects | sort or play |
| `ByTitle` | Arrange songs alphabetically | know where songs come from |
| `PygameOutput` | Produce sound | choose songs or order |
| `MusicPlayer` | Coordinate the steps | do any of the steps itself |

**Code: `Song` (models.py)**

```python
@dataclass(frozen=True)
class Song:
    title: str
    artist: str
    duration_sec: int = 0
    path: str | None = None
```

`Song` is pure data. It has no `play()` or `print()` method. If it did, a change in how sound is produced would force a change to the class that also represents data.

**Code: `FolderSongRepository` (library.py)**

```python
def get_all(self) -> list[Song]:
    songs = []
    for file in sorted(self._folder.glob("*")):
        if file.suffix.lower() not in self.EXTENSIONS:
            continue
        artist, sep, title = file.stem.partition(" - ")
        if not sep:
            artist, title = "Unknown", file.stem
        songs.append(Song(title.strip(), artist.strip(),
                          self._duration(file), str(file)))
    return songs
```

This class knows about folders and file names, and nothing else. It never plays, sorts or prints.

**Contrast with a naive design (illustrative, not part of the project).**

```python
class MusicPlayer:                      # violates SRP
    def play_all(self, folder, order):
        files = os.listdir(folder)            # job 1: finding songs
        if order == "title":                  # job 2: sorting
            files.sort()
        for f in files:
            print("Playing", f)               # job 3: presentation
            pygame.mixer.music.load(f)        # job 4: audio
            pygame.mixer.music.play()
```

This class has four reasons to change: the storage location, the sorting rules, the display text and the audio library. A change to any one risks breaking the other three, and none of them can be tested separately.

**Benefit in this project.** When `.m4a` support was discussed, the only change needed was one line in `FolderSongRepository` (`EXTENSIONS`). Nothing else was touched, because no other class knows which formats exist.

---

### 4.2 Open/Closed Principle (OCP)

**Definition.** Software entities should be open for extension but closed for modification. New behavior should be added by writing new code, not by editing code that already works.

**Where it is applied.** The player's behavior varies along three axes (where songs come from, in what order they play, how they are presented). Each axis is an abstraction with several implementations, and new variants are added as new classes.

**Code: the playback order extension point (ordering.py)**

```python
class PlaybackOrder(ABC):
    @abstractmethod
    def arrange(self, songs: list[Song]) -> list[Song]: ...


class InOrder(PlaybackOrder):
    def arrange(self, songs):
        return songs


class ByTitle(PlaybackOrder):
    def arrange(self, songs):
        return sorted(songs, key=lambda s: s.title)


class Shuffle(PlaybackOrder):
    def arrange(self, songs):
        shuffled = songs[:]
        random.shuffle(shuffled)
        return shuffled
```

`MusicPlayer` calls `self._order.arrange(...)` and has no idea which of the three it received.

**Contrast with a naive design (illustrative).**

```python
def arrange(self, songs, mode):          # violates OCP
    if mode == "title":
        return sorted(songs, key=lambda s: s.title)
    elif mode == "shuffle":
        ...
    # adding "by artist" means editing this existing function
```

Every new mode forces an edit to tested code, which risks regressions in the modes that already worked.

**Evidence from the project history.** The project demonstrates OCP directly:

1. The first version had `InMemorySongRepository` and `ConsoleOutput` only.
2. `LogFileOutput` was added later as a new class. `MusicPlayer` was not edited.
3. `FolderSongRepository` and `PygameOutput` (real audio) were added the same way. `MusicPlayer` was again unchanged. This is the strongest proof: the program went from printing text to playing real music without modifying the coordinator class.

**An honest caveat.** OCP applies to `MusicPlayer`, which stays closed. `main.py` is the "composition root" where concrete classes are chosen, so adding a new order also means adding one entry to the `ORDERS` dictionary there. Some place in any program must name the concrete classes; the principle keeps that place small and separate from the logic.

---

### 4.3 Dependency Inversion Principle (DIP)

**Definition.** High-level modules should not depend on low-level modules. Both should depend on abstractions. In the course wording: modules on different levels must not depend directly on each other, but rely on abstractions.

**Terminology.** `MusicPlayer` is *high-level* (it embodies the policy "get songs, order them, play them"). `PygameOutput` and `FolderSongRepository` are *low-level* (they deal with the audio library and the file system).

**Code: `MusicPlayer` (player.py)**

```python
class MusicPlayer:
    def __init__(self, repository: SongRepository,
                 order: PlaybackOrder, output: AudioOutput) -> None:
        self._repository = repository
        self._order = order
        self._output = output

    def play_all(self) -> None:
        for song in self._order.arrange(self._repository.get_all()):
            self._output.play(song)
```

Observe three facts:

1. The only imports in `player.py` are the three abstractions. The words `pygame`, `Folder` or `Shuffle` do not appear anywhere in the file.
2. The three collaborators are *injected* through the constructor. The class does not create them with `Something()`.
3. `play_all` is three lines because the real work lives in the injected objects.

**Code: the composition root (main.py)**

```python
output = ConsoleOutput() if args.silent else PygameOutput(args.preview or None)
player = MusicPlayer(FolderSongRepository(args.songs), ORDERS[args.order](), output)
player.play_all()
```

This is the single place where concrete classes are named and connected.

**Contrast with a naive design (illustrative).**

```python
class MusicPlayer:
    def __init__(self):
        self.output = PygameOutput()           # hard-wired to pygame
        self.repo = FolderSongRepository("songs")
```

Here the player cannot run without `pygame` installed and cannot be tested without real audio hardware and files. Replacing the audio library would require editing the player.

**Benefits observed in this project.**

- **`--silent` mode.** Because the output is injected, passing `ConsoleOutput` instead of `PygameOutput` lets the whole program run with no sound and no `pygame` installed. `pygame` is even imported lazily inside `PygameOutput.__init__` for this reason.
- **Testability.** A fake `AudioOutput` or a fake repository can be injected to test `MusicPlayer` in isolation (the project's playback logic was verified this way; see Section 7).
- **Swappability.** Replacing `pygame` with another library means writing one new `AudioOutput` subclass.

---

## 5. How the Three Principles Work Together

The principles reinforce each other:

- **SRP creates the seams.** Because storage, ordering and output are separate classes, there are natural places to insert abstractions.
- **DIP makes the seams pluggable.** The player talks to abstractions, so any implementation can be placed in a seam.
- **OCP is the payoff.** Because the seams are pluggable, new behavior arrives as new classes without touching working code.

Summary of one request's flow, "play my songs shuffled with sound":

```
main.py        builds FolderSongRepository, Shuffle, PygameOutput
               passes them into MusicPlayer
MusicPlayer    asks repository for songs      -> FolderSongRepository scans disk
               asks order to arrange them     -> Shuffle randomizes
               sends each song to output      -> PygameOutput plays audio
```

---

## 6. Extending the Project (Worked Example)

**Goal:** add an "alphabetical by artist" order.

**Step 1.** Add one class to `ordering.py`. No existing class is modified:

```python
class ByArtist(PlaybackOrder):
    def arrange(self, songs: list[Song]) -> list[Song]:
        return sorted(songs, key=lambda s: s.artist)
```

**Step 2.** Register it in `main.py` (composition root):

```python
from ordering import ByArtist, ByTitle, InOrder, Shuffle
ORDERS = {"inorder": InOrder, "title": ByTitle,
          "shuffle": Shuffle, "artist": ByArtist}
```

**Result:** `py main.py --order artist` works. `MusicPlayer`, the repositories and the outputs were not changed or retested.

The same recipe applies to a new output (for example a graphical display) or a new source (for example a playlist file).

---

## 7. Running and Verifying the Project

### 7.1 Running

```
py -m pip install pygame
py make_demo_songs.py            # optional: creates 3 demo tunes in songs/
py main.py                       # plays songs/ (20 seconds each)
py main.py --order shuffle       # inorder | title | shuffle | (artist, after Section 6)
py main.py --preview 0           # play full songs
py main.py --silent              # list only, no sound, no pygame required
```

To use your own music, copy `.mp3`, `.wav` or `.ogg` files into `songs/` and name them `Artist - Title.mp3`. Copy-protected formats such as `.m4p` cannot be played. For `.m4a`, add `".m4a"` to `EXTENSIONS` in `library.py` or convert to mp3.

### 7.2 What was verified

| Check | Method | Result |
|---|---|---|
| Song discovery and name parsing | Run in `--silent` mode with demo files and test files named `Artist - Title.mp3` | Artist and title extracted correctly |
| Ordering | `--silent --order title` and `--order shuffle` | Alphabetical and randomized output as expected |
| Playback logic of `PygameOutput` | Run against a stand-in (mock) of the `pygame` module | Each song was loaded, played and stopped in sequence |
| Real audio | Run by the project user on their own machine | The three demo songs played |

The player also illustrates a testing benefit of DIP: the mock-based check was possible only because `MusicPlayer` and `PygameOutput` depend on abstractions rather than on a hard-wired audio device.

---

## 8. Limitations and Future Work

1. **Two principles are not implemented.**
   - *Interface Segregation (I):* `AudioOutput` has a single method, so there is nothing to segregate yet. If pause, stop and volume were added, splitting them into small interfaces (for example `Playable` and `Pausable`) would let `LogFileOutput` avoid implementing methods it cannot support.
   - *Liskov Substitution (L):* The subclasses are used interchangeably and follow the contracts of their abstractions, but this was not examined as a separate topic. One known stress point: `PygameOutput.play` raises an error if a `Song` has no file path, which a `ConsoleOutput` would accept. A stricter design would make that requirement part of the abstraction's documented contract.
2. **Duration for mp3/ogg is unknown.** Reading it would need an extra library (for example `mutagen`); only `.wav` durations are read.
3. **File-name convention.** Metadata is parsed from `Artist - Title`; real mp3 tags (ID3) are ignored.
4. **No playback controls.** The program plays straight through; there is no pause, skip or volume.
5. **Not all concrete classes are reachable from the command line.** `InMemorySongRepository` and `LogFileOutput` exist to demonstrate extensibility but are not selected by `main.py`.
6. **No automated test suite.** The checks in Section 7 were manual; DIP makes writing unit tests straightforward and that would be the natural next step.

---

## 9. Conclusion

The project shows that SOLID principles are practical tools rather than abstract theory. By giving each class a single responsibility, the code became small and easy to read. By placing three abstractions at the points where behavior varies, new orders, sources and outputs could be added without editing the coordinating class. By injecting those abstractions into `MusicPlayer`, the high-level logic became independent of `pygame` and of the file system, which made silent mode and isolated testing possible. The strongest evidence is the project's own evolution: it changed from a program that printed text to one that plays real audio, and the class at its center, `MusicPlayer`, did not need to change.

---

## 10. Glossary

| Term | Meaning |
|---|---|
| **Abstraction / abstract base class** | A class that declares required methods without implementing them; used as a contract. |
| **Concrete class** | A class that implements the actual behavior. |
| **High-level module** | Code expressing the main policy or workflow. |
| **Low-level module** | Code dealing with details such as files, devices and libraries. |
| **Dependency** | Something a class needs in order to do its job. |
| **Dependency injection** | Supplying dependencies from outside (here via the constructor) instead of creating them inside. |
| **Composition root** | The single place where concrete objects are created and connected (`main.py`). |
| **Coupling** | How strongly one part of a program depends on another; lower is generally better. |
| **Cohesion** | How closely the contents of one class relate to a single purpose; higher is generally better. |

---

## 11. References

1. Course presentation, *SOLID Principles*, Software Design Techniques and Mechanisms.
2. Martin, R. C. (2000). *Design Principles and Design Patterns.*
3. Martin, R. C. (2008). *Clean Code: A Handbook of Agile Software Craftsmanship.* Prentice Hall.
4. "SOLID Principles: Explanation and Examples", ITNEXT. https://itnext.io/solid-principles-explanation-and-examples-715b975dcad4
5. "S.O.L.I.D: The First 5 Principles of Object Oriented Design", Scotch.io. https://scotch.io/bar-talk/s-o-l-i-d-the-first-five-principles-of-object-oriented-design
6. Python Software Foundation. *abc: Abstract Base Classes.* https://docs.python.org/3/library/abc.html
7. pygame documentation, `pygame.mixer.music`. https://www.pygame.org/docs/ref/music.html
