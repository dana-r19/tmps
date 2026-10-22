import argparse

from library import FolderSongRepository
from ordering import ByTitle, InOrder, Shuffle
from output import ConsoleOutput, PygameOutput
from player import MusicPlayer

ORDERS = {"inorder": InOrder, "title": ByTitle, "shuffle": Shuffle}


def main() -> None:
    parser = argparse.ArgumentParser(description="Tiny SOLID music player")
    parser.add_argument("--songs", default="songs", help="folder with audio files")
    parser.add_argument("--order", choices=ORDERS, default="inorder")
    parser.add_argument("--preview", type=int, default=20,
                        help="seconds to play per song (0 = full song)")
    parser.add_argument("--silent", action="store_true",
                        help="only list songs, no sound (no pygame needed)")
    args = parser.parse_args()

    # The only place where concrete classes are chosen (DIP)
    output = ConsoleOutput() if args.silent else PygameOutput(args.preview or None)
    player = MusicPlayer(FolderSongRepository(args.songs), ORDERS[args.order](), output)
    player.play_all()


if __name__ == "__main__":
    main()
