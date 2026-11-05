"""Credit entity and schema."""
from pydantic import BaseModel
from typing import Optional

class Credit(BaseModel):
    credit_id: str
    bundle_id: str
    status: str  # active, redeemed, expired
    route: str
    zone: str
    fare_family: str
    traveler_id: Optional[str]
    # TODO: Add more fields as per TSD
