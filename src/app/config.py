"""Environment-backed application settings."""
from dataclasses import dataclass
import os

@dataclass(frozen=True)
class Settings:
    database_url: str = os.getenv("DATABASE_URL", "sqlite:///insurance.db")
    host: str = os.getenv("HOST", "127.0.0.1")
    port: int = int(os.getenv("PORT", "5000"))
    debug: bool = os.getenv("FLASK_DEBUG", "false").lower() == "true"

    def as_flask_config(self) -> dict[str, object]:
        """Return settings consumed by Flask."""
        return {"DATABASE_URL": self.database_url, "DEBUG": self.debug}
