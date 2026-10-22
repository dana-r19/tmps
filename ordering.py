import random
from abc import ABC, abstractmethod
from models import Song


class PlaybackOrder(ABC):
    """OCP: the player is closed for modification, but you can extend
    playback behavior by adding a new subclass here (e.g. ByArtist, ShortestFirst)."""

    @abstractmethod
    def arrange(self, songs: list[Song]) -> list[Song]: ...


class InOrder(PlaybackOrder):
    def arrange(self, songs: list[Song]) -> list[Song]:
        return songs


class ByTitle(PlaybackOrder):
    def arrange(self, songs: list[Song]) -> list[Song]:
        return sorted(songs, key=lambda s: s.title)


class Shuffle(PlaybackOrder):
    def arrange(self, songs: list[Song]) -> list[Song]:
        shuffled = songs[:]
        random.shuffle(shuffled)
        return shuffled
