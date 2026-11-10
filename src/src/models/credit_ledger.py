"""
Pydantic schema for CreditLedger entry.
"""
from pydantic import BaseModel
from typing import Optional

class CreditLedgerEntry(BaseModel):
    entryId: str
    passId: str
    corporationId: str
    travelerId: str
    creditAction: str  # assign/redeem/cancel
    amount: int
    bookingRef: Optional[str]
    sabrePnr: Optional[str]
    emdNumber: Optional[str]
    timestamp: str
    status: str
