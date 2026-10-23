"""Notification log custom object for audit tracking."""
from typing import List

class NotificationLog:
    """Tracks notifications sent for an Opportunity approval chain."""
    def __init__(self, opportunity_id: str):
        self.opportunity_id = opportunity_id
        self.notifications: List[dict] = []  # Each: {to, channel, time, status}

    def add_notification(self, to: str, channel: str, time: str, status: str) -> None:
        # TODO: Append notification record
        raise NotImplementedError
