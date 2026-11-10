"""
User and role endpoints: profile, permissions, role management.
"""
from fastapi import APIRouter, Depends
from services.user_service import UserService
from core.security import require_roles

router = APIRouter(prefix="/user", tags=["user"])

@router.get("/profile")
async def get_profile(user=Depends(require_roles(["PassOwner", "Admin", "Arranger", "TravelAgent", "Traveler"]))):
    """Fetch user profile and permissions."""
    # TODO: call UserService.get_profile
    raise NotImplementedError
