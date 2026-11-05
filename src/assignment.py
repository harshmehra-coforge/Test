"""
Credit assignment models for Flight-Pass.
"""
from pydantic import BaseModel

class Assignment(BaseModel):
    id: str
    bundle_id: str
    user_id: str
    credits_assigned: int
    assigned_at: str
    # TODO: Add more fields as needed
