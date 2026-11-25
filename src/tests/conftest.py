"""Pytest fixtures for isolated in-memory SQLite tests."""
import pytest
from app import create_app
from app.config import Settings
from app.db import init_db

@pytest.fixture
def app(tmp_path):
    """Create an application backed by a fresh temporary database."""
    settings = Settings(database_url=f"sqlite:///{tmp_path / 'test.db'}")
    application = create_app(settings)
    application.config.update(TESTING=True)
    return application

@pytest.fixture
def client(app):
    """Return a Flask test client."""
    return app.test_client()
