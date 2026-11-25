"""Policy binding and retrieval service."""
from datetime import date
from uuid import uuid4
from ..errors import DomainError, NotFoundError, ConflictError
from ..models import Policy
from ..repositories import Repository

def bind_policy(repo: Repository, quote_id: str, coverage_start: date, coverage_end: date) -> Policy:
    """Bind an eligible quote into one active policy."""
    quote = repo.quote(quote_id)
    if quote is None:
        raise NotFoundError("QUOTE_NOT_FOUND", "Quote was not found.", 404)
    if quote.policy is not None:
        raise ConflictError("QUOTE_ALREADY_BOUND", "Quote has already been bound.", 409)
    if coverage_end <= coverage_start:
        raise DomainError("INVALID_COVERAGE_DATES", "Coverage end must be later than start.", field="coverage_end")
    return repo.add(Policy(policy_id=str(uuid4()), quote_id=quote.quote_id, driver_age=quote.driver_age, vehicle_type=quote.vehicle_type, premium=quote.premium, coverage_start=coverage_start, coverage_end=coverage_end, status="ACTIVE"))

def get_policy(repo: Repository, policy_id: str) -> Policy:
    """Return a policy or a stable not-found error."""
    policy = repo.policy(policy_id)
    if policy is None:
        raise NotFoundError("POLICY_NOT_FOUND", "Policy was not found.", 404)
    return policy
