"""
Pydantic schema for Booking entity.
"""
from pydantic import BaseModel
from typing import List, Optional

class Booking(BaseModel):
    bookingId: str
    passId: str
    travelerId: str
    itinerary: dict
    fareFamily: str
    routeZone: str
    seat: Optional[str]
    ancillaries: Optional[List[str]]
    status: str
    sabrePnr: Optional[str]
    emdDetails: Optional[dict]
    timestamps: dict
