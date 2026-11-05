"""Admin tools endpoints (credit/traveler/usage management)."""
from fastapi import APIRouter, Depends
from src.services.admin_service import AdminService
from src.core.security import require_roles

router = APIRouter(prefix="/admin", tags=["admin"])

@router.get("/dashboard")
async def admin_dashboard(user=Depends(require_roles(["PassOwner", "Admin"]))) -> dict:
    """Admin dashboard with credit, traveler, and usage stats."""
    # TODO: Call AdminService.get_dashboard
    raise NotImplementedError
