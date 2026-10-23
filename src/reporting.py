"""Audit/reporting queries for approval workflow."""
from typing import List

def get_deep_discount_approvals() -> List[dict]:
    """Return all Opportunities with >25% discount and their approval chains."""
    # TODO: Query Salesforce or custom object
    raise NotImplementedError
