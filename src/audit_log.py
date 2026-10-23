"""
Pydantic models for Audit Log entity.
"""
from pydantic import BaseModel
from typing import Optional
from datetime import datetime

class AuditLogEntry(BaseModel):
    id: str
    entity_type: str
    action: str
    actor: str
    timestamp: datetime
    event_detail: dict
    # TODO: Add additional fields for compliance/audit requirements
