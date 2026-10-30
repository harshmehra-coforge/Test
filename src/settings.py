"""
Configuration loader for Discount Approval Microservice
"""
import os
from dotenv import load_dotenv

load_dotenv()

class Settings:
    AZURE_COSMOS_URI: str = os.getenv("AZURE_COSMOS_URI", "")
    AZURE_COSMOS_KEY: str = os.getenv("AZURE_COSMOS_KEY", "")
    AZURE_COSMOS_DB: str = os.getenv("AZURE_COSMOS_DB", "")
    AZURE_COSMOS_CONTAINER: str = os.getenv("AZURE_COSMOS_CONTAINER", "")
    SALESFORCE_WEBHOOK_SECRET: str = os.getenv("SALESFORCE_WEBHOOK_SECRET", "")
    SLACK_BOT_TOKEN: str = os.getenv("SLACK_BOT_TOKEN", "")
    SLACK_CHANNEL_ID: str = os.getenv("SLACK_CHANNEL_ID", "")
    EMAIL_SENDER: str = os.getenv("EMAIL_SENDER", "")
    EMAIL_CONNECTION_STRING: str = os.getenv("EMAIL_CONNECTION_STRING", "")

settings = Settings()
