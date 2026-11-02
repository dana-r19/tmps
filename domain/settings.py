import threading


class Settings:
    """SINGLETON: the whole program shares exactly one Settings object.
    Settings() always returns the same instance (thread-safe, double-checked locking)."""

    _instance = None
    _lock = threading.Lock()

    def __new__(cls):
        if cls._instance is None:
            with cls._lock:
                if cls._instance is None:            # check again inside the lock
                    instance = super().__new__(cls)
                    instance.songs_folder = "songs"
                    instance.preview_seconds = 20    # 0 = play full songs
                    instance.backend = "auto"        # auto | simulated | pygame
                    instance.quick_mix_size = 5
                    cls._instance = instance
        return cls._instance
