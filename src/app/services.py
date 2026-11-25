"""Application use cases for quote, bind, FNOL, and status lookup."""
from datetime import date, timedelta
from typing import Any
from uuid import uuid4
from sqlalchemy.orm import Session

from .models import Claim, Policy, serialize_model
from .rating import DomainError, calculate_premium
from .repositories import SqlAlchemyClaimRepository, SqlAlchemyPolicyRepository


def quote(driver_age: int, vehicle_type: str) -> dict[str, Any]:
    """Return an indicative quote from authoritative rating rules."""
    result = calculate_premium(driver_age, vehicle_type)
    return {"quote_id": str(uuid4()), "premium": str(result.premium), "currency": result.currency,
            "rating_version": result.rating_version, "inputs": {"driver_age": result.driver_age, "vehicle_type": result.vehicle_type}}


def bind_policy(session: Session, driver_age: int, vehicle_type: str, effective_date: date, expiration_date: date) -> Policy:
    """Re-rate and persist an active policy in the caller's transaction."""
    if effective_date >= expiration_date:
        raise DomainError("INVALID_POLICY_DATES", "Effective date must be before expiration date.")
    result = calculate_premium(driver_age, vehicle_type)
    policy = Policy(driver_age=driver_age, vehicle_type=vehicle_type, premium=result.premium,
                    rating_version=result.rating_version, effective_date=effective_date,
                    expiration_date=expiration_date, status="ACTIVE")
    SqlAlchemyPolicyRepository(session).add(policy)
    return policy


def submit_claim(session: Session, policy_id: str, loss_date: date, loss_type: str, description: str, today: date | None = None) -> Claim:
    """Validate policy eligibility and create one FNOL claim."""
    policy = SqlAlchemyPolicyRepository(session).get(policy_id)
    if policy is None:
        raise DomainError("POLICY_NOT_FOUND", "Policy was not found.")
    current_date = today or date.today()
    if policy.status != "ACTIVE" or current_date > policy.expiration_date:
        raise DomainError("POLICY_NOT_CLAIM_ELIGIBLE", "Policy is not eligible for claims.")
    if loss_date > current_date:
        raise DomainError("FUTURE_LOSS_DATE", "Loss date cannot be in the future.", "loss_date")
    if not policy.effective_date <= loss_date <= policy.expiration_date:
        raise DomainError("LOSS_DATE_OUTSIDE_COVERAGE", "Loss date must fall within policy coverage.", "loss_date")
    if not loss_type.strip() or not description.strip() or len(description) > 2000:
        raise DomainError("INVALID_FNOL", "Loss type and description are required.")
    claim = Claim(policy_id=policy_id, loss_date=loss_date, reported_date=current_date,
                  loss_type=loss_type.strip(), description=description.strip(), status="FNOL_SUBMITTED")
    SqlAlchemyClaimRepository(session).add(claim)
    return claim


def get_claim(session: Session, claim_id: str) -> Claim | None:
    """Retrieve a claim without mutating persisted state."""
    return SqlAlchemyClaimRepository(session).get(claim_id)
