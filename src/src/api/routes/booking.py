"""
Booking endpoints: create, change, cancel, seat selection, ancillaries.
"""
from fastapi import APIRouter, Depends
from services.booking_service import BookingService
from core.security import require_roles

router = APIRouter(prefix="/booking", tags=["booking"])

@router.post("/create")
async def create_booking(data: dict, user=Depends(require_roles(["Arranger", "TravelAgent", "Traveler"]))):
    """Create a booking, reserve credits, and issue PNR/EMD-S."""
    # TODO: call BookingService.create_booking
    raise NotImplementedError

@router.post("/change")
async def change_booking(data: dict, user=Depends(require_roles(["Arranger", "TravelAgent", "Traveler"]))):
    """Modify, rebook, or cancel a booking."""
    # TODO: call BookingService.change_booking
    raise NotImplementedError
