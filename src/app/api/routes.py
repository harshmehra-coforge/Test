"""JSON API routes for quotes, policies, FNOL, and claim status."""
from datetime import date
from uuid import uuid4
from flask import Blueprint, jsonify, request
from ..db import database_ready, session_scope
from ..errors import DomainError
from ..repositories import Repository
from ..services.rating import create_quote
from ..services.policy import bind_policy, get_policy
from ..services.claim import submit_claim, get_claim

api = Blueprint("api", __name__)

def parse_date(value: object, field: str) -> date:
    """Parse a required ISO date into a domain date."""
    try:
        return date.fromisoformat(str(value))
    except (TypeError, ValueError) as exc:
        raise DomainError("INVALID_CLAIM_DATES", f"{field} must be YYYY-MM-DD.", 400, field) from exc

def response(entity: object) -> dict[str, object]:
    """Serialize supported domain entities without leaking ORM internals."""
    if hasattr(entity, "quote_id") and hasattr(entity, "driver_age"):
        return {"quote_id": entity.quote_id, "premium": f"{entity.premium:.2f}", "currency": "USD", "rating_version": entity.rating_version, "eligible": True}
    if hasattr(entity, "policy_id"):
        return {"policy_id": entity.policy_id, "quote_id": entity.quote_id, "premium": f"{entity.premium:.2f}", "status": entity.status, "coverage_start": entity.coverage_start.isoformat(), "coverage_end": entity.coverage_end.isoformat()}
    return {"claim_id": entity.claim_id, "policy_id": entity.policy_id, "loss_date": entity.loss_date.isoformat(), "report_date": entity.report_date.isoformat(), "status": entity.status}

def error_payload(exc: DomainError):
    return jsonify({"error": {"code": exc.code, "message": exc.message, "field": exc.field}, "request_id": str(uuid4())}), exc.status

@api.get("/healthz")
def health() -> tuple[object, int]:
    return jsonify({"status": "ok"}), 200

@api.get("/readyz")
def ready() -> tuple[object, int]:
    return jsonify({"status": "ready" if database_ready() else "not_ready"}), 200 if database_ready() else 503

@api.post("/quotes")
def quote_endpoint():
    try:
        body = request.get_json(silent=True) or {}
        with session_scope() as session:
            entity = create_quote(Repository(session), body.get("driver_age"), body.get("vehicle_type"))
        return jsonify(response(entity)), 201
    except DomainError as exc:
        return error_payload(exc)

@api.post("/policies")
def policy_endpoint():
    try:
        body = request.get_json(silent=True) or {}
        with session_scope() as session:
            entity = bind_policy(Repository(session), str(body.get("quote_id")), parse_date(body.get("coverage_start"), "coverage_start"), parse_date(body.get("coverage_end"), "coverage_end"))
        return jsonify(response(entity)), 201
    except DomainError as exc:
        return error_payload(exc)

@api.get("/policies/<policy_id>")
def policy_get_endpoint(policy_id: str):
    try:
        with session_scope() as session: entity = get_policy(Repository(session), policy_id)
        return jsonify(response(entity)), 200
    except DomainError as exc: return error_payload(exc)

@api.post("/claims")
def claim_endpoint():
    try:
        body = request.get_json(silent=True) or {}
        with session_scope() as session:
            entity = submit_claim(Repository(session), str(body.get("policy_id")), parse_date(body.get("loss_date"), "loss_date"), parse_date(body.get("report_date"), "report_date"), str(body.get("loss_description", "")))
        return jsonify(response(entity)), 201
    except DomainError as exc: return error_payload(exc)

@api.get("/claims/<claim_id>")
def claim_get_endpoint(claim_id: str):
    try:
        with session_scope() as session: entity = get_claim(Repository(session), claim_id)
        return jsonify(response(entity)), 200
    except DomainError as exc: return error_payload(exc)
