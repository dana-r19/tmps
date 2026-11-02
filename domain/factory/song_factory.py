import wave
from abc import ABC, abstractmethod
from pathlib import Path

from domain.models.song import M4aSong, Mp3Song, OggSong, Song, WavSong


class SongLoader(ABC):
    """FACTORY METHOD - the Creator.
    load() contains the shared logic (parse 'Artist - Title' from the file name) and
    calls the factory method create_song(); each subclass decides WHICH Song to create."""

    def load(self, path) -> Song:
        path = Path(path)
        artist, sep, title = path.stem.partition(" - ")
        if not sep:
            artist, title = "Unknown", path.stem
        return self.create_song(title.strip(), artist.strip(), path)

    @abstractmethod
    def create_song(self, title: str, artist: str, path: Path) -> Song:
        """The factory method."""


class Mp3SongLoader(SongLoader):
    def create_song(self, title, artist, path):
        return Mp3Song(title, artist, 0, str(path))


class WavSongLoader(SongLoader):
    def create_song(self, title, artist, path):
        with wave.open(str(path)) as w:                  # wav headers contain the length
            duration = int(w.getnframes() / w.getframerate())
        return WavSong(title, artist, duration, str(path))


class OggSongLoader(SongLoader):
    def create_song(self, title, artist, path):
        return OggSong(title, artist, 0, str(path))


class M4aSongLoader(SongLoader):
    def create_song(self, title, artist, path):
        return M4aSong(title, artist, 0, str(path))


# Supporting a new format = one new SongLoader subclass + one line here.
_LOADERS: dict[str, SongLoader] = {
    ".mp3": Mp3SongLoader(),
    ".wav": WavSongLoader(),
    ".ogg": OggSongLoader(),
    ".m4a": M4aSongLoader(),
}


def supported_extensions() -> set[str]:
    return set(_LOADERS)


def loader_for(path) -> SongLoader:
    extension = Path(path).suffix.lower()
    if extension not in _LOADERS:
        raise ValueError(f"Unsupported audio format: '{extension}'")
    return _LOADERS[extension]
