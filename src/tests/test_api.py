"""REST contract smoke tests for core endpoints."""
def test_quote_endpoint_returns_contract(client):
    response = client.post('/api/v1/quotes', json={'driver_age': 30, 'vehicle_type': 'sedan'})
    assert response.status_code == 201
    assert response.get_json()['premium'] == '600.00'

def test_underage_quote_is_not_persisted(client):
    response = client.post('/api/v1/quotes', json={'driver_age': 17, 'vehicle_type': 'sedan'})
    assert response.status_code == 422
    assert response.get_json()['error']['code'] == 'DRIVER_UNDERAGE'
