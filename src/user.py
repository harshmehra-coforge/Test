"""User entity and schema."""
from pydantic import BaseModel
from typing import List

class User(BaseModel):
    user_id: str
    roles: List[str]
    organization: str
    allocations: List[str]
    # TODO: Add more fields as per TSD
