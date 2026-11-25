"""
App configuration loader for environment variables and defaults.
"""
import os

def get_config():
    """Return config object for Flask app."""
    # TODO: Add environment variable parsing as needed
    class Config:
        SQLALCHEMY_DATABASE_URI = os.getenv("DATABASE_URL", "sqlite:///app.db")
        SQLALCHEMY_TRACK_MODIFICATIONS = False
        SECRET_KEY = os.getenv("SECRET_KEY", "dev")
    return Config
