"""FNOL submission and claim-status service."""
from datetime import date
from uuid import uuid4
from ..errors import DomainError, NotFoundError
from ..models import Claim
from ..repositories import Repository

def submit_claim(repo: Repository, policy_id: str, loss_date: date, report_date: date, description: str) -> Claim:
    """Validate coverage and persist a submitted FNOL claim."""
    policy = repo.policy(policy_id)
    if policy is None:
        raise NotFoundError("POLICY_NOT_FOUND", "Policy was not found.", 404)
    if policy.status != "ACTIVE":
        raise DomainError("POLICY_NOT_CLAIM_ELIGIBLE", "Policy is not eligible for claims.", 422)
    if not policy.coverage_start <= loss_date <= policy.coverage_end:
        raise DomainError("LOSS_OUTSIDE_COVERAGE", "Loss date is outside coverage.", 422, "loss_date")
    if report_date < loss_date:
        raise DomainError("REPORT_BEFORE_LOSS", "Report date cannot precede loss date.", 422, "report_date")
    if not 10 <= len(description.strip()) <= 2000:
        raise DomainError("INVALID_DESCRIPTION", "Description must contain 10 to 2000 characters.", 422, "loss_description")
    return repo.add(Claim(claim_id=str(uuid4()), policy_id=policy_id, loss_date=loss_date, report_date=report_date, loss_description=description.strip(), status="SUBMITTED"))

def get_claim(repo: Repository, claim_id: str) -> Claim:
    """Return a claim status record or a stable not-found error."""
    claim = repo.claim(claim_id)
    if claim is None:
        raise NotFoundError("CLAIM_NOT_FOUND", "Claim was not found.", 404)
    return claim
