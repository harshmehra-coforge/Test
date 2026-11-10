"""
Error and audit models for error tracking and audit trail.
"""
from pydantic import BaseModel
from typing import Optional

class Error(BaseModel):
    id: str
    transaction_id: str
    error_type: str
    error_code: Optional[str] = None
    message: Optional[str] = None
    created_at: str

class AuditTrail(BaseModel):
    id: str
    action: str
    user: str
    details: Optional[str] = None
    timestamp: str
