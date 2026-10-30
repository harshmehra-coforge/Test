"""
Opportunity data model
"""
from pydantic import BaseModel
from typing import Optional

class Opportunity(BaseModel):
    id: str
    owner: str
    discount_percent: float
    status: str
    approval_stage: str
    comments: Optional[str] = None
