"""Reporting endpoints (usage, financials, compliance)."""
from fastapi import APIRouter, Depends
from src.services.reporting_service import ReportingService
from src.core.security import require_roles

router = APIRouter(prefix="/reporting", tags=["reporting"])

@router.get("/usage")
async def usage_report(user=Depends(require_roles(["PassOwner", "Admin"]))) -> dict:
    """Get usage report for credits and travelers."""
    # TODO: Call ReportingService.usage_report
    raise NotImplementedError

@router.get("/financials")
async def financial_report(user=Depends(require_roles(["PassOwner", "Admin"]))) -> dict:
    """Get financial report for bundles and credits."""
    # TODO: Call ReportingService.financial_report
    raise NotImplementedError
