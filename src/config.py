"""Environment-backed configuration."""
from functools import lru_cache
from pydantic_settings import BaseSettings, SettingsConfigDict

class Settings(BaseSettings):
    """Runtime settings."""
    app_name: str = "basic-calculator"
    environment: str = "local"
    debug: bool = False
    model_config = SettingsConfigDict(env_prefix="CALCULATOR_", env_file=".env", extra="ignore")

@lru_cache
def get_settings() -> Settings:
    """Return cached settings."""
    return Settings()
