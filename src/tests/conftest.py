"""Shared isolated Flask and SQLite fixtures for API tests."""
import pytest
from app import create_app
from app.config import Settings
from app.extensions import db


@pytest.fixture
def app(tmp_path):
    """Create an app backed by a temporary SQLite database."""
    settings = Settings(database_url=f"sqlite:///{tmp_path / 'test.db'}", testing=True)
    application = create_app(settings)
    yield application
    with application.app_context():
        db.drop_all()


@pytest.fixture
def client(app):
    """Return a Flask test client."""
    return app.test_client()
