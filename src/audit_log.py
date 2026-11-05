"""
Audit log models for Flight-Pass.
"""
from pydantic import BaseModel

class AuditLog(BaseModel):
    id: str
    user_id: str
    action: str
    timestamp: str
    details: dict
    # TODO: Add more fields as needed
