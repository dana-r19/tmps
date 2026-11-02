import copy
from dataclasses import dataclass, field

from domain.models.song import Song


@dataclass
class Playlist:
    name: str
    songs: list[Song] = field(default_factory=list)

    def clone(self, new_name: str | None = None) -> "Playlist":
        """PROTOTYPE: create a new playlist by copying an existing one (deep copy),
        instead of building it again from scratch."""
        duplicate = copy.deepcopy(self)
        if new_name:
            duplicate.name = new_name
        return duplicate
