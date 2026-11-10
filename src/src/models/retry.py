"""
Retry and RetryRequest models for retry attempts and orchestration.
"""
from pydantic import BaseModel
from typing import Optional

class Retry(BaseModel):
    id: str
    transaction_id: str
    attempt: int
    status: str
    error_code: Optional[str] = None
    created_at: str

class RetryRequest(BaseModel):
    transaction_id: str
    attempt: int
    error_code: Optional[str] = None
