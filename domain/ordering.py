import random
from abc import ABC, abstractmethod

from domain.models.song import Song


class PlaybackOrder(ABC):
    @abstractmethod
    def arrange(self, songs: list[Song]) -> list[Song]: ...


class InOrder(PlaybackOrder):
    def arrange(self, songs):
        return list(songs)


class ByTitle(PlaybackOrder):
    def arrange(self, songs):
        return sorted(songs, key=lambda s: s.title)


class Shuffle(PlaybackOrder):
    def arrange(self, songs):
        shuffled = list(songs)
        random.shuffle(shuffled)
        return shuffled
