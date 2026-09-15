"""Model tests."""
from calculator.models import CalculationRequest, Operation

def test_request_accepts_operation() -> None:
    """Supported operation validates."""
    request = CalculationRequest(left=2, right=3, operation=Operation.ADD)
    assert request.operation is Operation.ADD
