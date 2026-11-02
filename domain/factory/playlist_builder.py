from pathlib import Path

from domain.factory.song_factory import loader_for, supported_extensions
from domain.models.playlist import Playlist
from domain.models.song import Song
from domain.ordering import ByTitle, InOrder, PlaybackOrder, Shuffle
from domain.settings import Settings


class PlaylistBuilder:
    """BUILDER: assembles a Playlist step by step (name, songs, order, size limit).
    Every step returns self, so calls can be chained. build() produces the result."""

    def __init__(self) -> None:
        self.reset()

    def reset(self) -> "PlaylistBuilder":
        self._name = "Untitled"
        self._songs: list[Song] = []
        self._order: PlaybackOrder = InOrder()
        self._limit: int | None = None
        return self

    def named(self, name: str) -> "PlaylistBuilder":
        self._name = name
        return self

    def add_song(self, song: Song) -> "PlaylistBuilder":
        self._songs.append(song)
        return self

    def add_file(self, path) -> "PlaylistBuilder":
        self._songs.append(loader_for(path).load(path))      # uses the Factory Method
        return self

    def add_folder(self, folder) -> "PlaylistBuilder":
        for file in sorted(Path(folder).glob("*")):
            if file.suffix.lower() in supported_extensions():
                self.add_file(file)
        return self

    def ordered_by(self, order: PlaybackOrder) -> "PlaylistBuilder":
        self._order = order
        return self

    def limited_to(self, count: int | None) -> "PlaylistBuilder":
        self._limit = count
        return self

    def build(self) -> Playlist:
        songs = self._order.arrange(self._songs)
        if self._limit:
            songs = songs[: self._limit]
        playlist = Playlist(self._name, songs)
        self.reset()                                   # builder can be reused
        return playlist


class PlaylistDirector:
    """DIRECTOR (part of the Builder pattern): knows ready-made recipes
    for using the builder's steps in a fixed sequence."""

    def __init__(self, builder: PlaylistBuilder) -> None:
        self._builder = builder

    def quick_mix(self, folder, size: int | None = None) -> Playlist:
        size = size or Settings().quick_mix_size          # Singleton in use
        return (self._builder.named("Quick mix").add_folder(folder)
                .ordered_by(Shuffle()).limited_to(size).build())

    def alphabetical(self, folder) -> Playlist:
        return (self._builder.named("A-Z").add_folder(folder)
                .ordered_by(ByTitle()).build())
