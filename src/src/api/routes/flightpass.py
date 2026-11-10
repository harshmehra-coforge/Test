"""
FlightPass endpoints: purchase, allocate, query passes and bundles.
"""
from fastapi import APIRouter, Depends
from services.flightpass_service import FlightPassService
from core.security import require_roles

router = APIRouter(prefix="/flightpass", tags=["flightpass"])

@router.post("/buy")
async def buy_flightpass(data: dict, user=Depends(require_roles(["PassOwner"]))):
    """Purchase a flight pass bundle and assign to a corporation."""
    # TODO: call FlightPassService.buy_pass
    raise NotImplementedError

@router.patch("/{id}/allocate")
async def allocate_credits(id: str, data: dict, user=Depends(require_roles(["PassOwner", "Admin"]))):
    """Allocate credits to users/roles."""
    # TODO: call FlightPassService.allocate_credits
    raise NotImplementedError

@router.get("/{id}/ledger")
async def get_ledger(id: str, user=Depends(require_roles(["PassOwner", "Admin", "Arranger"]))):
    """Query credits ledger for a pass."""
    # TODO: call FlightPassService.get_ledger
    raise NotImplementedError
