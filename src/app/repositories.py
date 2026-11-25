"""Repository adapters isolating SQLAlchemy persistence from application services."""
from typing import Protocol
from sqlalchemy import select
from sqlalchemy.orm import Session

from .models import Claim, Policy


class PolicyRepository(Protocol):
    """Contract for policy persistence."""
    def get(self, policy_id: str) -> Policy | None: ...
    def add(self, policy: Policy) -> Policy: ...


class ClaimRepository(Protocol):
    """Contract for claim persistence."""
    def get(self, claim_id: str) -> Claim | None: ...
    def add(self, claim: Claim) -> Claim: ...


class SqlAlchemyPolicyRepository:
    """SQLAlchemy implementation for policy records."""
    def __init__(self, session: Session) -> None:
        self.session = session

    def get(self, policy_id: str) -> Policy | None:
        return self.session.scalar(select(Policy).where(Policy.policy_id == policy_id))

    def add(self, policy: Policy) -> Policy:
        # TODO: enforce additional database checks/migrations as the schema evolves.
        self.session.add(policy)
        return policy


class SqlAlchemyClaimRepository:
    """SQLAlchemy implementation for claim records."""
    def __init__(self, session: Session) -> None:
        self.session = session

    def get(self, claim_id: str) -> Claim | None:
        return self.session.scalar(select(Claim).where(Claim.claim_id == claim_id))

    def add(self, claim: Claim) -> Claim:
        self.session.add(claim)
        return claim
