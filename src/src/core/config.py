"""
Configuration loader for environment variables and Azure Key Vault integration.
Uses pydantic BaseSettings for 12-factor config.
"""
from pydantic import BaseSettings, Field
from typing import List, Optional

class Settings(BaseSettings):
    ENV: str = Field("dev", env="ENV")
    POSTGRES_DSN: str = Field(..., env="POSTGRES_DSN")
    COSMOSDB_URI: str = Field(..., env="COSMOSDB_URI")
    COSMOSDB_KEY: str = Field(..., env="COSMOSDB_KEY")
    SERVICE_BUS_CONN_STR: str = Field(..., env="SERVICE_BUS_CONN_STR")
    STRIPE_API_KEY: Optional[str] = Field(None, env="STRIPE_API_KEY")
    PAYPAL_CLIENT_ID: Optional[str] = Field(None, env="PAYPAL_CLIENT_ID")
    PAYPAL_CLIENT_SECRET: Optional[str] = Field(None, env="PAYPAL_CLIENT_SECRET")
    SUPPORTED_CURRENCIES: List[str] = Field(default_factory=list, env="SUPPORTED_CURRENCIES")
    RETRY_MAX_ATTEMPTS: int = Field(3, env="RETRY_MAX_ATTEMPTS")
    RETRY_INTERVAL_SEC: int = Field(30, env="RETRY_INTERVAL_SEC")
    KEY_VAULT_URI: Optional[str] = Field(None, env="KEY_VAULT_URI")

    class Config:
        env_file = ".env"


def get_settings() -> Settings:
    """Return app settings (singleton)."""
    # TODO: Integrate with Azure Key Vault for secrets
    return Settings()
