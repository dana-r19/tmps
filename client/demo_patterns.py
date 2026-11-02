"""Shows every creational pattern in action, without playing any sound."""
import sys
import tempfile
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from domain.factory.playlist_builder import PlaylistBuilder, PlaylistDirector
from domain.factory.song_factory import loader_for
from domain.ordering import ByTitle
from domain.playback.player import describe
from domain.settings import Settings

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")


def title(text: str) -> None:
    print(f"\n=== {text} ===")


title("1. SINGLETON")
a, b = Settings(), Settings()
print("a is b:", a is b)
a.preview_seconds = 7
print("changed through a, read through b:", b.preview_seconds)
a.preview_seconds = 20

title("2. FACTORY METHOD")
folder = Settings().songs_folder
with tempfile.TemporaryDirectory() as tmp:
    files = [Path(f) for f in sorted(Path(folder).glob("*.wav"))[:1]]
    for ext in (".mp3", ".ogg", ".m4a"):                  # empty stand-in files
        fake = Path(tmp) / f"Some Artist - Some Title{ext}"
        fake.touch()
        files.append(fake)
    for file in files:
        loader = loader_for(file)
        song = loader.load(file)
        print(f"{file.suffix:<5} -> {type(loader).__name__:<15} creates {type(song).__name__}")

title("3. BUILDER (step by step)")
builder = PlaylistBuilder()
mine = (builder.named("Study").add_folder(folder).ordered_by(ByTitle()).limited_to(2).build())
print(mine.name, "->", [s.title for s in mine.songs])
print("same builder, reused:", builder.named("Empty").build())

title("3b. DIRECTOR (ready-made recipe)")
mix = PlaylistDirector(builder).quick_mix(folder, size=3)
print(mix.name, "->", [describe(s) for s in mix.songs])

title("4. PROTOTYPE (clone instead of rebuilding)")
copy_of_mine = mine.clone("Study (copy)")
copy_of_mine.songs.pop()
print("original:", mine.name, len(mine.songs), "songs")
print("clone   :", copy_of_mine.name, len(copy_of_mine.songs), "songs")
print("independent lists:", mine.songs is not copy_of_mine.songs)
