"""Configuration loader using environment variables."""
from pydantic import BaseSettings
import os

class Settings(BaseSettings):
    azure_client_id: str = os.getenv("AZURE_CLIENT_ID", "")
    azure_tenant_id: str = os.getenv("AZURE_TENANT_ID", "")
    azure_client_secret: str = os.getenv("AZURE_CLIENT_SECRET", "")
    azure_subscription_id: str = os.getenv("AZURE_SUBSCRIPTION_ID", "")
    azure_resource_group: str = os.getenv("AZURE_RESOURCE_GROUP", "")
    azure_container_registry: str = os.getenv("AZURE_CONTAINER_REGISTRY", "")
    azure_cosmos_uri: str = os.getenv("AZURE_COSMOS_URI", "")
    azure_cosmos_key: str = os.getenv("AZURE_COSMOS_KEY", "")
    azure_blob_conn_string: str = os.getenv("AZURE_BLOB_CONN_STRING", "")
    azure_service_bus_conn_str: str = os.getenv("AZURE_SERVICE_BUS_CONN_STR", "")
    azure_key_vault_uri: str = os.getenv("AZURE_KEY_VAULT_URI", "")
    postgres_host: str = os.getenv("POSTGRES_HOST", "localhost")
    postgres_port: int = int(os.getenv("POSTGRES_PORT", 5432))
    postgres_db: str = os.getenv("POSTGRES_DB", "")
    postgres_user: str = os.getenv("POSTGRES_USER", "")
    postgres_password: str = os.getenv("POSTGRES_PASSWORD", "")
    secret_key: str = os.getenv("SECRET_KEY", "")


def get_settings() -> Settings:
    """Return app settings."""
    return Settings()
