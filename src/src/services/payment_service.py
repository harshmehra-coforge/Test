"""
PaymentService orchestrates payment processing, gateway selection, idempotency, and state recording.
"""
from src.models.transaction import PaymentRequest, PaymentResponse

class PaymentService:
    def __init__(self):
        # TODO: Inject repositories, gateways, config
        pass

    async def process_payment(self, payment: PaymentRequest) -> PaymentResponse:
        """Process a payment request, select gateway, handle idempotency, record state."""
        # TODO: Implement payment orchestration logic
        raise NotImplementedError
