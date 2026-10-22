import time
from abc import ABC, abstractmethod

from models import Song


class AudioOutput(ABC):
    """Abstraction (DIP): high-level player code never knows how sound is 'played'."""

    @abstractmethod
    def play(self, song: Song) -> None: ...


def _label(song: Song) -> str:
    text = f"{song.title} - {song.artist}"
    if song.duration_sec:
        m, s = divmod(song.duration_sec, 60)
        text += f" [{m}:{s:02d}]"
    return text


class ConsoleOutput(AudioOutput):
    def play(self, song: Song) -> None:
        print(f"  ♪ {_label(song)}")


class LogFileOutput(AudioOutput):
    def __init__(self, path: str) -> None:
        self._path = path

    def play(self, song: Song) -> None:
        with open(self._path, "a", encoding="utf-8") as f:
            f.write(f"{song.title} - {song.artist}\n")


class PygameOutput(AudioOutput):
    """Plays real audio through pygame. Needs: pip install pygame.
    preview_seconds limits each song so a demo doesn't take an hour (None = full song)."""

    def __init__(self, preview_seconds: int | None = 20) -> None:
        import pygame                      # imported here so the rest works without it
        self._pygame = pygame
        pygame.mixer.init()
        self._limit = preview_seconds

    def play(self, song: Song) -> None:
        if not song.path:
            raise ValueError(f"'{song.title}' has no audio file to play")
        print(f"  ▶ Now playing: {_label(song)}")
        music = self._pygame.mixer.music
        music.load(song.path)
        music.play()
        start = time.time()
        while music.get_busy():
            if self._limit and time.time() - start >= self._limit:
                break
            time.sleep(0.1)
        music.stop()
