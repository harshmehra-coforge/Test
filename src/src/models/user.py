"""
Pydantic schema for UserProfile entity.
"""
from pydantic import BaseModel
from typing import List, Optional

class UserProfile(BaseModel):
    userId: str
    roles: List[str]
    corporationId: str
    contactInfo: Optional[dict]
    loyaltyId: Optional[str]
    permissions: List[str]
