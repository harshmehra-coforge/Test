"""
FastAPI router for notification endpoints.
"""
from fastapi import APIRouter, Depends, HTTPException
from src.service.notification_service import NotificationService
from src.models.notification import NotificationEvent

router = APIRouter()

@router.post("/send", response_model=None)
async def send_notification(event: NotificationEvent, service: NotificationService = Depends()):
    """Send a notification event."""
    # TODO: Wire up service and implement logic
    raise NotImplementedError
