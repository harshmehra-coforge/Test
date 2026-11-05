"""
Admin and reporting endpoints for Flight-Pass.
Handles dashboards, reporting, and export features.
"""
from fastapi import APIRouter, Depends
from services.admin_service import AdminService
from core.rbac import require_roles, Role

router = APIRouter(prefix="/admin", tags=["admin"])

@router.get("/dashboard")
async def get_dashboard(user_role: Role = Depends()):
    """Get admin dashboard data."""
    # TODO: Call AdminService, aggregate/report data
    raise NotImplementedError
