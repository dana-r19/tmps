from dataclasses import dataclass


@dataclass(frozen=True)
class Song:
    """SRP: this class only holds song data. No printing, no storage, no playback."""
    title: str
    artist: str
    duration_sec: int = 0      # 0 = unknown
    path: str | None = None    # audio file on disk, if any
