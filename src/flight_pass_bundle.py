"""FlightPassBundle entity and schema."""
from pydantic import BaseModel
from typing import List, Optional

class FlightPassBundle(BaseModel):
    bundle_id: str
    purchaser_org_id: str
    credits: int
    expiry_date: str
    routes: List[str]
    zones: List[str]
    fare_families: List[str]
    # TODO: Add more fields as per TSD
