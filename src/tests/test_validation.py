"""Required claim validation tests asserting invalid writes are not committed."""
from datetime import date, timedelta


def _policy(client, effective: date, expiration: date) -> str:
    response = client.post('/api/v1/policies', json={'driver_age': 40, 'vehicle_type': 'suv', 'effective_date': effective.isoformat(), 'expiration_date': expiration.isoformat()})
    assert response.status_code == 201
    return response.get_json()['policy_id']


def test_claim_before_effective_date_is_rejected_without_write(client):
    """Loss before coverage returns 422 and does not create a claim."""
    today = date.today()
    policy_id = _policy(client, today, today + timedelta(days=30))
    response = client.post('/api/v1/claims', json={'policy_id': policy_id, 'loss_date': (today - timedelta(days=1)).isoformat(), 'loss_type': 'collision', 'description': 'Front impact'})
    assert response.status_code == 422
    assert response.get_json()['error']['code'] == 'LOSS_DATE_OUTSIDE_COVERAGE'


def test_expired_policy_is_rejected_without_write(client):
    """Expired coverage returns 409 and does not create a claim."""
    today = date.today()
    policy_id = _policy(client, today - timedelta(days=30), today - timedelta(days=1))
    response = client.post('/api/v1/claims', json={'policy_id': policy_id, 'loss_date': (today - timedelta(days=2)).isoformat(), 'loss_type': 'theft', 'description': 'Vehicle missing'})
    assert response.status_code == 409
    assert response.get_json()['error']['code'] == 'POLICY_NOT_CLAIM_ELIGIBLE'
