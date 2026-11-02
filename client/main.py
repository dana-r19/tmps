import argparse
import sys
import threading
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))   # so 'domain' is importable

from domain.factory.playlist_builder import PlaylistBuilder, PlaylistDirector
from domain.ordering import ByTitle, InOrder, Shuffle
from domain.playback.backends import PygameBackend, SimulatedBackend
from domain.playback.player import MusicPlayer, describe
from domain.settings import Settings

ORDERS = {"inorder": InOrder, "title": ByTitle, "shuffle": Shuffle}


def create_backend(name: str):
    if name in ("auto", "pygame"):
        try:
            return PygameBackend()
        except ImportError:
            if name == "pygame":
                sys.exit("pygame is not installed:  py -m pip install pygame")
            print("(pygame not found: simulated playback, no real sound)")
    return SimulatedBackend()


def listen_for_commands(player: MusicPlayer) -> None:
    for line in sys.stdin:
        command = line.strip().lower()
        if command == "p":
            player.toggle()
        elif command == "s":
            player.skip()
        elif command == "q":
            player.stop()
            return


def main() -> None:
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8", errors="replace")

    parser = argparse.ArgumentParser(description="Music player (creational patterns lab)")
    parser.add_argument("--songs", help="folder with audio files")
    parser.add_argument("--order", choices=ORDERS, default="inorder")
    parser.add_argument("--preset", choices=["quickmix", "alphabetical"],
                        help="use a ready-made recipe from the Director")
    parser.add_argument("--limit", type=int, help="max number of songs")
    parser.add_argument("--preview", type=int, help="seconds per song (0 = full)")
    parser.add_argument("--backend", choices=["auto", "pygame", "simulated"])
    parser.add_argument("--list", action="store_true", help="only list, don't play")
    args = parser.parse_args()

    settings = Settings()                                  # Singleton
    if args.songs:
        settings.songs_folder = args.songs
    if args.preview is not None:
        settings.preview_seconds = args.preview
    if args.backend:
        settings.backend = args.backend

    builder = PlaylistBuilder()                            # Builder
    if args.preset == "quickmix":
        playlist = PlaylistDirector(builder).quick_mix(settings.songs_folder, args.limit)
    elif args.preset == "alphabetical":
        playlist = PlaylistDirector(builder).alphabetical(settings.songs_folder)
    else:
        playlist = (builder.named("My playlist")
                    .add_folder(settings.songs_folder)     # Factory Method inside
                    .ordered_by(ORDERS[args.order]())
                    .limited_to(args.limit)
                    .build())

    if not playlist.songs:
        sys.exit(f"No songs found in '{settings.songs_folder}'. Run: py make_demo_songs.py")

    if args.list:
        print(f"Playlist '{playlist.name}':")
        for song in playlist.songs:
            print("  ♪", describe(song))
        return

    player = MusicPlayer(create_backend(settings.backend), settings.preview_seconds or None)
    print("Controls (type then press Enter):  p = pause/resume   s = skip   q = quit")
    threading.Thread(target=listen_for_commands, args=(player,), daemon=True).start()
    player.play(playlist)


if __name__ == "__main__":
    main()
