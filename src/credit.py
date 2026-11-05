"""
Credit assignment endpoints for Flight-Pass.
Handles assigning credits to users and audit logging.
"""
from fastapi import APIRouter, Depends
from services.credit_service import CreditService
from core.rbac import require_roles, Role

router = APIRouter(prefix="/credit", tags=["credit"])

@router.post("/assign")
async def assign_credits(assignment_data: dict, user_role: Role = Depends()):
    """Assign credits to a user."""
    # TODO: Validate input, call CreditService, audit assignment
    raise NotImplementedError
