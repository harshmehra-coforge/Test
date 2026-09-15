"""HTTP route tests."""
from fastapi.testclient import TestClient

def test_health_endpoint(client: TestClient) -> None:
    """Health endpoint returns liveness."""
    response = client.get("/health")
    assert response.status_code == 200
    assert response.json() == {"status": "ok"}

def test_calculate_contract_is_pending(client: TestClient) -> None:
    """TODO: Verify result after service implementation."""
    response = client.post("/v1/calculate", json={"left": 8, "right": 2, "operation": "divide"})
    assert response.status_code == 500
