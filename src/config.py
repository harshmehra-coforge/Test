"""
Configuration management for environment variables and settings.
"""
import os
from pydantic import BaseSettings

class Settings(BaseSettings):
    cosmos_db_uri: str
    cosmos_db_key: str
    cosmos_db_database: str
    event_hub_conn_str: str
    notification_email_api_key: str
    slack_webhook_url: str
    jwt_secret: str
    azure_managed_identity: str
    class Config:
        env_file = ".env"

settings = Settings()
