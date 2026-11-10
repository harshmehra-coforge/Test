"""
/status endpoint routes. Returns transaction status by reference or id.
"""
from fastapi import APIRouter, Query
from src.models.transaction import PaymentStatusResponse

router = APIRouter(prefix="/status", tags=["status"])

@router.get("/")
async def get_status(
    transaction_id: str = Query(..., description="Transaction ID")
) -> PaymentStatusResponse:
    """Return the status of a payment transaction."""
    # TODO: Query transaction status from repository
    raise NotImplementedError
