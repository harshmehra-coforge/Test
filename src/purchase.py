"""
Purchase endpoints for Flight-Pass bundles.
Handles bundle purchase, payment, and audit logging.
"""
from fastapi import APIRouter, Depends
from services.purchase_service import PurchaseService
from core.rbac import require_roles, Role

router = APIRouter(prefix="/purchase", tags=["purchase"])

@router.post("/bundle")
async def purchase_bundle(bundle_data: dict, user_role: Role = Depends()):
    """Purchase a flight-pass bundle."""
    # TODO: Validate input, call PurchaseService, handle payment/audit
    raise NotImplementedError
