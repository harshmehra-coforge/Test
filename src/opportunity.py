"""Opportunity object extension for discount approval workflow."""
from typing import Optional, List

class Opportunity:
    """Represents a Salesforce Opportunity with approval workflow fields."""
    def __init__(self, id: str, discount: float, approval_status: str, approval_chain: List[str], sla_timestamps: dict):
        self.id = id
        self.discount = discount
        self.approval_status = approval_status
        self.approval_chain = approval_chain
        self.sla_timestamps = sla_timestamps
        # TODO: Add additional fields as needed
