"""Dashboard endpoints for user interaction."""
from fastapi import APIRouter

router = APIRouter(prefix="/dashboard", tags=["dashboard"])

@router.get("/")
async def dashboard_home():
    """Dashboard landing page."""
    # TODO: Return dashboard summary/status
    raise NotImplementedError
