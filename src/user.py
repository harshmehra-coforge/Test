"""
User and role models for BizWeb Flight-Pass.
"""
from pydantic import BaseModel
from core.rbac import Role

class User(BaseModel):
    id: str
    name: str
    email: str
    role: Role
    organization_id: str
    # TODO: Add more fields as needed
