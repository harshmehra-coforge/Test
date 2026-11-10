"""
Pydantic schema for FlightPass entity.
"""
from pydantic import BaseModel
from typing import List, Optional

class FlightPass(BaseModel):
    passId: str
    corporationId: str
    bundleSize: int
    validityStart: str
    validityEnd: str
    fareFamilies: List[str]
    routesZones: List[str]
    creditList: Optional[list] = None
    status: str
