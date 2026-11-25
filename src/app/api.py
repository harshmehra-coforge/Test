"""Versioned REST endpoints and safe domain-error mapping."""
from datetime import date
from uuid import uuid4
from flask import Blueprint, jsonify, render_template, request, g
from .extensions import db
from .models import Policy, serialize_model
from .rating import DomainError
from .services import bind_policy, get_claim, quote, submit_claim

api_bp = Blueprint("api", __name__)


@api_bp.before_request
def assign_correlation_id() -> None:
    """Attach a request correlation ID for safe diagnostics."""
    g.correlation_id = request.headers.get("X-Correlation-ID", str(uuid4()))


def _error(error: DomainError, status: int = 422):
    return jsonify({"error": {"code": error.code, "message": error.message, "correlation_id": g.correlation_id, "field": error.field}}), status


def _payload() -> dict:
    """Parse an object JSON body and reject malformed requests."""
    data = request.get_json(silent=True)
    if not isinstance(data, dict):
        raise DomainError("INVALID_JSON", "Request body must be a JSON object.")
    return data


@api_bp.get("/")
def index():
    """Serve the self-contained browser UI."""
    return render_template("index.html")


@api_bp.get("/healthz")
def healthz():
    """Return a minimal process health response."""
    return jsonify({"status": "ok"})


@api_bp.post("/api/v1/quotes")
def create_quote():
    """Calculate an indicative premium."""
    try:
        data = _payload()
        return jsonify(quote(data["driver_age"], data["vehicle_type"])), 201
    except KeyError as exc:
        return _error(DomainError("INVALID_JSON", f"Missing field: {exc.args[0]}"))
    except DomainError as exc:
        return _error(exc)


@api_bp.post("/api/v1/policies")
def create_policy():
    """Bind a policy after server-side re-rating and date validation."""
    try:
        data = _payload()
        policy = bind_policy(db.session, data["driver_age"], data["vehicle_type"], date.fromisoformat(data["effective_date"]), date.fromisoformat(data["expiration_date"]))
        db.session.commit()
        return jsonify(serialize_model(policy)), 201
    except (KeyError, ValueError) as exc:
        db.session.rollback()
        return _error(DomainError("INVALID_POLICY_DATES", "Policy fields or dates are invalid."))
    except DomainError as exc:
        db.session.rollback()
        return _error(exc)


@api_bp.get("/api/v1/policies/<policy_id>")
def read_policy(policy_id: str):
    """Return a policy by opaque identifier."""
    policy = db.session.get(Policy, policy_id)
    return (jsonify(serialize_model(policy)), 200) if policy else _error(DomainError("POLICY_NOT_FOUND", "Policy was not found."), 404)


@api_bp.post("/api/v1/claims")
def create_claim():
    """Register a validated FNOL claim."""
    try:
        data = _payload()
        claim = submit_claim(db.session, data["policy_id"], date.fromisoformat(data["loss_date"]), data["loss_type"], data["description"])
        db.session.commit()
        return jsonify(serialize_model(claim)), 201
    except KeyError as exc:
        db.session.rollback()
        return _error(DomainError("INVALID_FNOL", f"Missing field: {exc.args[0]}"))
    except ValueError:
        db.session.rollback()
        return _error(DomainError("INVALID_FNOL", "Dates and FNOL fields are invalid."))
    except DomainError as exc:
        db.session.rollback()
        return _error(exc, 404 if exc.code == "POLICY_NOT_FOUND" else 409 if exc.code == "POLICY_NOT_CLAIM_ELIGIBLE" else 422)


@api_bp.get("/api/v1/claims/<claim_id>")
def read_claim(claim_id: str):
    """Retrieve claim status without a write."""
    claim = get_claim(db.session, claim_id)
    return (jsonify(serialize_model(claim)), 200) if claim else _error(DomainError("CLAIM_NOT_FOUND", "Claim was not found."), 404)
