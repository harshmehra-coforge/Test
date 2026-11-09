"""Job schema/model for fine-tune/build/deploy jobs."""
from pydantic import BaseModel
from typing import Optional, Dict

class Job(BaseModel):
    job_id: str
    user_id: str
    type: str  # fine-tune/build/deploy
    status: str
    params: Dict[str, str]
    timestamps: Dict[str, str]
    logs_ref: Optional[str]
