"""Booking entity and schema."""
from pydantic import BaseModel
from typing import Optional

class Booking(BaseModel):
    booking_id: str
    pnr_id: str
    traveler_id: str
    credit_id: str
    itinerary: dict
    status: str
    # TODO: Add more fields as per TSD
