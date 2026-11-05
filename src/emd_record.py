"""EMDRecord entity and schema."""
from pydantic import BaseModel
from typing import Optional

class EMDRecord(BaseModel):
    emd_id: str
    booking_id: str
    credit_id: str
    issued_at: str
    status: str
    # TODO: Add more fields as per TSD
