"""
AuditLog data model
"""
from pydantic import BaseModel
from typing import Any, Dict
from datetime import datetime

class AuditLog(BaseModel):
    record_type: str
    entity_id: str
    action: str
    user: str
    timestamp: datetime
    metadata: Dict[str, Any]
