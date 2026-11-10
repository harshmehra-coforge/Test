"""
CurrencyProcessor validates, normalizes, and converts currencies as required by gateways.
"""
from src.models.transaction import PaymentRequest

class CurrencyProcessor:
    def __init__(self):
        # TODO: Inject config, currency helpers
        pass

    def validate_and_normalize(self, payment: PaymentRequest) -> str:
        """Validate and normalize currency for the payment request."""
        # TODO: Implement currency validation/normalization
        raise NotImplementedError
