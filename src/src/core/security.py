"""
Security and RBAC helpers for JWT and Azure AD integration.
"""
from fastapi import Depends, HTTPException, status
from fastapi.security import OAuth2PasswordBearer
from typing import List

# TODO: Implement JWT validation and Azure AD RBAC

oauth2_scheme = OAuth2PasswordBearer(tokenUrl="token")

def get_current_user(token: str = Depends(oauth2_scheme)):
    """Validate JWT and return user claims."""
    # TODO: Decode JWT, validate, and extract user info
    raise NotImplementedError

def require_roles(roles: List[str]):
    """Dependency to enforce RBAC on endpoints."""
    def wrapper(user=Depends(get_current_user)):
        # TODO: Check user roles
        raise NotImplementedError
    return wrapper
