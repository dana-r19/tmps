import wave
from abc import ABC, abstractmethod
from pathlib import Path

from models import Song


class SongRepository(ABC):
    """Abstraction (DIP): the player depends on this, not on a concrete storage."""

    @abstractmethod
    def get_all(self) -> list[Song]: ...


class InMemorySongRepository(SongRepository):
    """SRP: its only job is to provide songs (hardcoded, no audio)."""

    def __init__(self) -> None:
        self._songs = [
            Song("Bohemian Rhapsody", "Queen", 354),
            Song("Billie Jean", "Michael Jackson", 294),
        ]

    def get_all(self) -> list[Song]:
        return list(self._songs)


class FolderSongRepository(SongRepository):
    """Reads audio files from a folder. Files must be named 'Artist - Title.mp3'
    (.wav and .ogg work too). Added without modifying MusicPlayer (OCP)."""

    EXTENSIONS = {".mp3", ".wav", ".ogg", ".m4a"}

    def __init__(self, folder: str) -> None:
        self._folder = Path(folder)

    def get_all(self) -> list[Song]:
        songs = []
        for file in sorted(self._folder.glob("*")):
            if file.suffix.lower() not in self.EXTENSIONS:
                continue
            artist, sep, title = file.stem.partition(" - ")
            if not sep:                      # no "Artist - " prefix in the name
                artist, title = "Unknown", file.stem
            songs.append(Song(title.strip(), artist.strip(),
                              self._duration(file), str(file)))
        return songs

    @staticmethod
    def _duration(file: Path) -> int:
        if file.suffix.lower() == ".wav":
            with wave.open(str(file)) as w:
                return int(w.getnframes() / w.getframerate())
        elif file.suffix.lower() == ".m4a":
            # Placeholder for M4A duration calculation (requires extra library like mutagen)
            return 0
        return 0  # mp3/ogg: unknown without extra libraries
