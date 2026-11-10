"""
Admin/reporting endpoints: usage, booking, financial reports.
"""
from fastapi import APIRouter, Depends
from services.admin_service import AdminService
from core.security import require_roles

router = APIRouter(prefix="/admin", tags=["admin"])

@router.get("/reports/usage")
async def get_usage_report(user=Depends(require_roles(["PassOwner", "Admin"]))):
    """Fetch credit, booking, and financial usage reports."""
    # TODO: call AdminService.get_usage_report
    raise NotImplementedError
