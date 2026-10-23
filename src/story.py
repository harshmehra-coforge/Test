"""
Pydantic models for Story/Opportunity entity.
"""
from pydantic import BaseModel, Field
from typing import List, Optional
from datetime import datetime

class SubmissionHistory(BaseModel):
    reviewer_tier: str
    status: str
    timestamp: datetime
    feedback_id: Optional[str]

class Story(BaseModel):
    id: str
    tenant_id: str
    opportunity_id: str
    current_status: str
    last_reviewer_tier: str
    resubmission_count: int
    submission_history: List[SubmissionHistory]
    feedback_refs: List[str]
    created_at: datetime
    updated_at: datetime
    # TODO: Add additional fields as required by business logic
