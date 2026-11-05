"""
Booking models for Flight-Pass.
"""
from pydantic import BaseModel

class Booking(BaseModel):
    id: str
    user_id: str
    bundle_id: str
    flight_number: str
    booking_date: str
    status: str
    # TODO: Add more fields as needed
