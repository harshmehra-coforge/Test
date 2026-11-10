"""
/payments endpoint routes. Handles payment requests and delegates to payment service.
"""
from fastapi import APIRouter, Depends, HTTPException, status
from src.services.payment_service import PaymentService
from src.models.transaction import PaymentRequest, PaymentResponse

router = APIRouter(prefix="/payments", tags=["payments"])

@router.post("/", response_model=PaymentResponse)
async def create_payment(
    payment: PaymentRequest,
    service: PaymentService = Depends(PaymentService)
):
    """Process a payment request via the selected gateway."""
    # TODO: Validate, call PaymentService.process_payment
    raise NotImplementedError
