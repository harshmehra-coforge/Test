"""Bundle purchase and management endpoints."""
from fastapi import APIRouter, Depends
from src.services.bundle_service import BundleService
from src.core.security import require_roles

router = APIRouter(prefix="/bundles", tags=["bundles"])

@router.post("/")
async def purchase_bundle(bundle_data: dict, user=Depends(require_roles(["PassOwner", "Admin"]))) -> dict:
    """Purchase a new Flight-Pass bundle."""
    # TODO: Call BundleService.purchase_bundle
    raise NotImplementedError

@router.get("/{bundle_id}")
async def get_bundle(bundle_id: str, user=Depends(require_roles(["PassOwner", "Admin", "Arranger", "Agent"]))) -> dict:
    """Get details of a Flight-Pass bundle."""
    # TODO: Call BundleService.get_bundle
    raise NotImplementedError
