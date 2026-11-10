"""
Credit ledger endpoints: refund, audit, usage queries.
"""
from fastapi import APIRouter, Depends
from services.credit_ledger_service import CreditLedgerService
from core.security import require_roles

router = APIRouter(prefix="/credit", tags=["credit"])

@router.post("/refund")
async def refund_credit(data: dict, user=Depends(require_roles(["PassOwner", "Admin"]))):
    """Credit return flow, enforce audit."""
    # TODO: call CreditLedgerService.refund_credit
    raise NotImplementedError
