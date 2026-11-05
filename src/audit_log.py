"""AuditLog entity and schema."""
from pydantic import BaseModel
from typing import Optional

class AuditLog(BaseModel):
    event_id: str
    user_id: str
    action: str
    timestamp: str
    entity_affected: str
    # TODO: Add more fields as per TSD
