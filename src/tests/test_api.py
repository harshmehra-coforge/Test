"""HTTP smoke tests for health and read-only lookup contracts."""
def test_healthz(client):
    """Health endpoint returns a minimal successful response."""
    response = client.get('/healthz')
    assert response.status_code == 200
    assert response.get_json()['status'] == 'ok'


def test_unknown_claim_returns_not_found(client):
    """Unknown claim identifiers do not fabricate data."""
    response = client.get('/api/v1/claims/not-a-real-claim')
    assert response.status_code == 404
    assert response.get_json()['error']['code'] == 'CLAIM_NOT_FOUND'
