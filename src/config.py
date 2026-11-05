"""App configuration and environment variable management."""
from pydantic import BaseSettings
from typing import Optional

class Settings(BaseSettings):
    """App settings loaded from environment variables."""
    ENV: str = "development"
    COSMOS_DB_URI: str
    COSMOS_DB_KEY: str
    POSTGRES_URI: str
    REDIS_URI: str
    SABRE_PSS_API_URL: str
    EMD_S_API_URL: str
    WESTJET_DIGITAL_API_URL: str
    WESTJET_LOYALTY_API_URL: str
    AZURE_AD_TENANT_ID: str
    AZURE_AD_CLIENT_ID: str
    AZURE_AD_CLIENT_SECRET: str
    KEY_VAULT_URI: str
    class Config:
        env_file = ".env"

settings = Settings()
