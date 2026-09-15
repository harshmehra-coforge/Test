"""Shared pytest fixtures."""
import pytest
from fastapi.testclient import TestClient
from calculator.main import create_app

@pytest.fixture
def client() -> TestClient:
    """Return a test client."""
    return TestClient(create_app())
