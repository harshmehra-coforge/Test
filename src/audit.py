"""Audit log schema/model."""
from pydantic import BaseModel
from typing import Optional

class AuditLog(BaseModel):
    action_id: str
    user_id: str
    action_type: str
    resource: str
    detail: Optional[str]
    timestamp: str
