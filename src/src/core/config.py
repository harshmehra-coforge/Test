"""
Configuration loader using pydantic BaseSettings for environment variables.
"""
from pydantic import BaseSettings
from typing import Optional

class Settings(BaseSettings):
    COSMOS_DB_URI: str
    COSMOS_DB_KEY: str
    REDIS_URL: str
    AZURE_KEY_VAULT_URI: str
    EVENT_HUBS_CONN_STR: str
    JWT_SECRET: str
    AZURE_AD_TENANT_ID: str
    PCI_PAYMENT_PROVIDER_URL: Optional[str] = None
    class Config:
        env_file = ".env"

def get_settings() -> Settings:
    """Return app settings from environment."""
    return Settings()
