from dataclasses import dataclass
from typing import ClassVar


@dataclass(frozen=True)
class Song:
    """Base product. Plain data, one subclass per audio format (created by the Factory Method)."""
    title: str
    artist: str
    duration_sec: int = 0          # 0 = unknown
    path: str | None = None
    FORMAT: ClassVar[str] = "?"


class Mp3Song(Song):
    FORMAT = "MP3"


class WavSong(Song):
    FORMAT = "WAV"


class OggSong(Song):
    FORMAT = "OGG"


class M4aSong(Song):
    FORMAT = "M4A"
