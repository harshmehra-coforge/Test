"""Environment-backed settings for local and WSGI execution."""
from dataclasses import dataclass
import os


@dataclass(frozen=True)
class Settings:
    """Runtime configuration with safe localhost defaults."""

    database_url: str = os.getenv("DATABASE_URL", "sqlite:///insurance.db")
    secret_key: str = os.getenv("SECRET_KEY", "change-me-in-development")
    max_content_length: int = 64 * 1024
    testing: bool = os.getenv("TESTING", "0") == "1"

    def as_flask_config(self) -> dict[str, object]:
        """Return Flask-compatible configuration values."""
        return {"SQLALCHEMY_DATABASE_URI": self.database_url, "SQLALCHEMY_TRACK_MODIFICATIONS": False,
                "SECRET_KEY": self.secret_key, "MAX_CONTENT_LENGTH": self.max_content_length,
                "TESTING": self.testing}
