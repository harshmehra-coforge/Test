"""
Transaction, PaymentRequest, PaymentResponse, and PaymentStatusResponse models.
"""
from pydantic import BaseModel, Field
from typing import Optional, List

class PaymentRequest(BaseModel):
    amount: float
    currency: str
    gateway: str
    payer_id: str
    idempotency_key: str
    # TODO: Add additional fields as needed

class PaymentResponse(BaseModel):
    transaction_id: str
    status: str
    message: Optional[str] = None

class PaymentStatusResponse(BaseModel):
    transaction_id: str
    status: str
    retries: int
    last_error: Optional[str] = None

class Transaction(BaseModel):
    id: str
    amount: float
    currency: str
    gateway: str
    payer_id: str
    status: str
    created_at: str
    updated_at: str
    # TODO: Add additional fields as needed
