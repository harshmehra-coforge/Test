"""Mandatory coverage validation tests and no-invalid-write assertions."""
from datetime import date
import pytest
from app.db import session_scope
from app.errors import DomainError
from app.models import Claim, Policy
from app.repositories import Repository
from app.services.claim import submit_claim

def _policy(status="ACTIVE"):
    return Policy(policy_id="p1", quote_id="q1", driver_age=30, vehicle_type="sedan", premium="600.00", coverage_start=date(2026,1,1), coverage_end=date(2026,12,31), status=status)

def test_loss_before_coverage_start_is_rejected(app):
    with session_scope() as session:
        session.add(_policy())
    with pytest.raises(DomainError, match="outside coverage"):
        with session_scope() as session: submit_claim(Repository(session), "p1", date(2025,12,31), date(2026,1,2), "Rear end collision")
    with session_scope() as session: assert session.query(Claim).count() == 0

def test_loss_after_coverage_end_or_expired_policy_is_rejected(app):
    with session_scope() as session: session.add(_policy(status="EXPIRED"))
    with pytest.raises(DomainError, match="not eligible"):
        with session_scope() as session: submit_claim(Repository(session), "p1", date(2026,12,31), date(2026,12,31), "Rear end collision")
    with session_scope() as session: assert session.query(Claim).count() == 0
