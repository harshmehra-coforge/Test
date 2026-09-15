"""Calculator application service."""
from calculator.models import CalculationRequest, CalculationResponse

class CalculationError(ValueError):
    """Raised when a calculation cannot be performed."""

class CalculatorService:
    """Perform arithmetic independently of HTTP."""
    def calculate(self, request: CalculationRequest) -> CalculationResponse:
        """Calculate one operation."""
        # TODO: Implement operation dispatch and divide-by-zero handling.
        raise NotImplementedError
