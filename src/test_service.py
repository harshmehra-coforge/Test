"""Service test skeleton."""
import pytest
from calculator.models import CalculationRequest, Operation
from calculator.service import CalculatorService

def test_calculation_is_pending() -> None:
    """TODO: Assert arithmetic result after implementation."""
    request = CalculationRequest(left=2, right=3, operation=Operation.ADD)
    with pytest.raises(NotImplementedError):
        CalculatorService().calculate(request)
