"""
Notification endpoints for Flight-Pass.
Handles sending notifications (expiry, booking, etc.).
"""
from fastapi import APIRouter, Depends
from services.notification_service import NotificationService
from core.rbac import require_roles, Role

router = APIRouter(prefix="/notification", tags=["notification"])

@router.post("/send")
async def send_notification(notification_data: dict, user_role: Role = Depends()):
    """Send a notification to a user."""
    # TODO: Validate input, call NotificationService
    raise NotImplementedError
