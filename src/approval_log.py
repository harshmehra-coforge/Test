"""Approval chain summary and comments."""
from typing import List

class ApprovalLog:
    """Tracks approval/rejection decisions and comments for an Opportunity."""
    def __init__(self, opportunity_id: str):
        self.opportunity_id = opportunity_id
        self.entries: List[dict] = []  # Each entry: {tier, decision, comments, timestamp}

    def add_entry(self, tier: str, decision: str, comments: str, timestamp: str) -> None:
        # TODO: Append entry to log
        raise NotImplementedError
