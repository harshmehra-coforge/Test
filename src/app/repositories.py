"""Repository boundary for domain persistence operations."""
from sqlalchemy import select
from sqlalchemy.orm import Session
from .models import Quote, Policy, Claim

class Repository:
    """Small session-backed repository used by application services."""
    def __init__(self, session: Session) -> None:
        self.session = session
    def quote(self, quote_id: str) -> Quote | None:
        return self.session.scalar(select(Quote).where(Quote.quote_id == quote_id))
    def policy(self, policy_id: str) -> Policy | None:
        return self.session.scalar(select(Policy).where(Policy.policy_id == policy_id))
    def claim(self, claim_id: str) -> Claim | None:
        return self.session.scalar(select(Claim).where(Claim.claim_id == claim_id))
    def add(self, entity: object) -> object:
        self.session.add(entity)
        self.session.flush()
        return entity
