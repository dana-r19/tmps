from library import SongRepository
from ordering import PlaybackOrder
from output import AudioOutput


class MusicPlayer:
    """
    SRP: only coordinates playback (doesn't store, sort or render anything itself).
    OCP: new orders / outputs are plugged in, this class never changes.
    DIP: depends only on abstractions, all injected through the constructor.
    """

    def __init__(self, repository: SongRepository,
                 order: PlaybackOrder, output: AudioOutput) -> None:
        self._repository = repository
        self._order = order
        self._output = output

    def play_all(self) -> None:
        for song in self._order.arrange(self._repository.get_all()):
            self._output.play(song)
