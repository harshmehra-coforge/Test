"""Credit management and redemption endpoints."""
from fastapi import APIRouter, Depends
from src.services.credit_service import CreditService
from src.core.security import require_roles

router = APIRouter(prefix="/credits", tags=["credits"])

@router.post("/redeem")
async def redeem_credit(redeem_data: dict, user=Depends(require_roles(["Arranger", "Agent", "Traveler"]))) -> dict:
    """Redeem a flight credit for booking."""
    # TODO: Call CreditService.redeem_credit
    raise NotImplementedError

@router.get("/{credit_id}")
async def get_credit(credit_id: str, user=Depends(require_roles(["PassOwner", "Admin", "Arranger", "Agent", "Traveler"]))) -> dict:
    """Get details of a credit."""
    # TODO: Call CreditService.get_credit
    raise NotImplementedError
