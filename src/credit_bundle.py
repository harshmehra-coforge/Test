"""
Credit bundle models for Flight-Pass.
"""
from pydantic import BaseModel
from typing import List

class CreditBundle(BaseModel):
    id: str
    organization_id: str
    credits_total: int
    credits_remaining: int
    valid_from: str
    valid_to: str
    route_zone: str
    fare_family: str
    # TODO: Add more fields as needed
