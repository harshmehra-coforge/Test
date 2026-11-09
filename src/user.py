"""User, role, session, and permissions models."""
from pydantic import BaseModel
from typing import Optional, List

class UserProfile(BaseModel):
    user_id: str
    name: str
    email: str
    roles: List[str]

class Role(BaseModel):
    role_id: str
    name: str
    permissions: List[str]

class Session(BaseModel):
    session_id: str
    user_id: str
    expires_at: str

class Permission(BaseModel):
    permission_id: str
    name: str
