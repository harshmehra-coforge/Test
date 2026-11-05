"""
Role-based access control utilities for BizWeb Flight-Pass.
Defines roles and permission checks for endpoints.
"""
from enum import Enum
from fastapi import Depends, HTTPException, status
from typing import List

class Role(str, Enum):
    PASS_OWNER = "pass_owner"
    ADMIN = "admin"
    ARRANGER = "arranger"
    TRAVEL_AGENT = "travel_agent"
    TRAVELER = "traveler"

# TODO: Implement Azure AD integration for RBAC

def require_roles(allowed_roles: List[Role]):
    def dependency(user_role: Role = Depends()):
        if user_role not in allowed_roles:
            raise HTTPException(status_code=status.HTTP_403_FORBIDDEN, detail="Insufficient permissions")
    return dependency
