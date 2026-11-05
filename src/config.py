"""
Configuration module for BizWeb Flight-Pass microservices.
Loads settings from environment variables using Pydantic BaseSettings.
"""
from pydantic import BaseSettings

class Settings(BaseSettings):
    app_name: str = "BizWeb Flight-Pass"
    environment: str = "development"
    debug: bool = True
    postgres_dsn: str
    cosmos_dsn: str
    redis_url: str
    eventhub_conn_str: str
    azure_ad_tenant: str
    azure_ad_client_id: str
    azure_ad_client_secret: str
    # Add more as needed

    class Config:
        env_file = ".env"

settings = Settings()
