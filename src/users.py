"""User and role management endpoints."""
from fastapi import APIRouter, Depends
from src.services.user_service import UserService
from src.core.security import require_roles

router = APIRouter(prefix="/users", tags=["users"])

@router.get("/")
async def list_users(user=Depends(require_roles(["PassOwner", "Admin"]))) -> list:
    """List users in the organization."""
    # TODO: Call UserService.list_users
    raise NotImplementedError

@router.post("/assign-role")
async def assign_role(role_data: dict, user=Depends(require_roles(["PassOwner", "Admin"]))) -> dict:
    """Assign a role to a user."""
    # TODO: Call UserService.assign_role
    raise NotImplementedError
