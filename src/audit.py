"""
FastAPI router for audit log endpoints.
"""
from fastapi import APIRouter, Depends, HTTPException
from src.service.audit_log_service import AuditLogService
from src.models.audit_log import AuditLogEntry

router = APIRouter()

@router.post("/log", response_model=None)
async def log_event(entry: AuditLogEntry, service: AuditLogService = Depends()):
    """Log an auditable event."""
    # TODO: Wire up service and implement logic
    raise NotImplementedError
