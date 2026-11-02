import threading
import time
from typing import Callable

from domain.models.playlist import Playlist
from domain.models.song import Song
from domain.playback.backends import AudioBackend


def describe(song: Song) -> str:
    text = f"{song.title} - {song.artist} [{song.FORMAT}]"
    if song.duration_sec:
        minutes, seconds = divmod(song.duration_sec, 60)
        text += f" {minutes}:{seconds:02d}"
    return text


class MusicPlayer:
    """Plays a Playlist through an injected AudioBackend and supports
    pause / resume / skip / stop (they can be called from another thread)."""

    def __init__(self, backend: AudioBackend, preview_seconds: int | None = None,
                 on_event: Callable[[str], None] = print) -> None:
        self._backend = backend
        self._limit = preview_seconds
        self._emit = on_event
        self._paused = False
        self._skip = threading.Event()
        self._quit = threading.Event()

    # ---- controls ----
    def pause(self) -> None:
        if not self._paused:
            self._paused = True
            self._backend.pause()
            self._emit("  ⏸  Paused")

    def resume(self) -> None:
        if self._paused:
            self._paused = False
            self._backend.resume()
            self._emit("  ▶  Resumed")

    def toggle(self) -> None:
        self.resume() if self._paused else self.pause()

    def skip(self) -> None:
        self._skip.set()

    def stop(self) -> None:
        self._quit.set()

    # ---- playback ----
    def play(self, playlist: Playlist) -> None:
        total = len(playlist.songs)
        self._emit(f"Playlist '{playlist.name}' ({total} songs)")
        for number, song in enumerate(playlist.songs, 1):
            if self._quit.is_set():
                break
            self._play_song(number, total, song)
        self._emit("Finished." if not self._quit.is_set() else "Stopped.")

    def _play_song(self, number: int, total: int, song: Song) -> None:
        self._skip.clear()
        self._paused = False
        self._emit(f"▶ [{number}/{total}] {describe(song)}")
        self._backend.start(song)
        played, last = 0.0, time.monotonic()
        while not self._quit.is_set():
            time.sleep(0.05)
            now = time.monotonic()
            if not self._paused:
                played += now - last          # paused time does not count
            last = now
            if self._skip.is_set():
                self._emit("  ⏭  Skipped")
                break
            if self._paused:
                continue
            if self._backend.has_finished():
                break
            if self._limit and played >= self._limit:
                break
        self._backend.stop()
