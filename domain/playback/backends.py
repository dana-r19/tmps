import time
from abc import ABC, abstractmethod

from domain.models.song import Song


class AudioBackend(ABC):
    """Abstraction the player depends on (Dependency Inversion)."""

    @abstractmethod
    def start(self, song: Song) -> None: ...

    @abstractmethod
    def pause(self) -> None: ...

    @abstractmethod
    def resume(self) -> None: ...

    @abstractmethod
    def stop(self) -> None: ...

    @abstractmethod
    def has_finished(self) -> bool: ...


class SimulatedBackend(AudioBackend):
    """No sound and no libraries: just a clock. Great for testing pause/skip."""

    def __init__(self, unknown_length: int = 8) -> None:
        self._unknown_length = unknown_length
        self._length = 0.0
        self._played = 0.0
        self._since = 0.0
        self._paused = False

    def start(self, song):
        self._length = song.duration_sec or self._unknown_length
        self._played, self._since, self._paused = 0.0, time.monotonic(), False

    def pause(self):
        if not self._paused:
            self._played += time.monotonic() - self._since
            self._paused = True

    def resume(self):
        if self._paused:
            self._since, self._paused = time.monotonic(), False

    def stop(self):
        self._length = 0.0

    def has_finished(self):
        played = self._played + (0 if self._paused else time.monotonic() - self._since)
        return played >= self._length


class PygameBackend(AudioBackend):
    """Real audio through pygame (pip install pygame)."""

    def __init__(self) -> None:
        import pygame                      # imported here so the rest works without it
        pygame.mixer.init()
        self._music = pygame.mixer.music
        self._paused = False

    def start(self, song):
        if not song.path:
            raise ValueError(f"'{song.title}' has no audio file")
        self._paused = False
        self._music.load(song.path)
        self._music.play()

    def pause(self):
        self._music.pause()
        self._paused = True

    def resume(self):
        self._music.unpause()
        self._paused = False

    def stop(self):
        self._music.stop()
        self._paused = False

    def has_finished(self):
        return not self._paused and not self._music.get_busy()
