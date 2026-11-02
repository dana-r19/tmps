"""Creates 4 short synthesized demo tunes in ./songs (original melodies, not real songs)."""
import math
import struct
import wave
from pathlib import Path

RATE = 22050
NOTES = {"C": 261.63, "D": 293.66, "E": 329.63, "F": 349.23,
         "G": 392.00, "A": 440.00, "B": 493.88, "C5": 523.25}
TUNES = {
    "Demo Artist - Going Up":   "C D E F G A B C5",
    "Demo Artist - Going Down": "C5 B A G F E D C",
    "Demo Artist - Hop Around": "C E G E C G C5 G",
    "Demo Artist - Wobble":     "C E C E G E G C5",
}


def write_tune(path: Path, notes: str, repeat: int = 3, note_len: float = 0.4) -> None:
    frames = bytearray()
    for name in notes.split() * repeat:
        freq, n = NOTES[name], int(RATE * note_len)
        for i in range(n):
            fade = min(1, i / 500, (n - i) / 500)
            frames += struct.pack("<h", int(12000 * fade * math.sin(2 * math.pi * freq * i / RATE)))
    with wave.open(str(path), "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(RATE)
        w.writeframes(bytes(frames))


if __name__ == "__main__":
    folder = Path(__file__).resolve().parent / "songs"
    folder.mkdir(exist_ok=True)
    for name, notes in TUNES.items():
        write_tune(folder / f"{name}.wav", notes)
    print(f"Created {len(TUNES)} demo songs in {folder}")
