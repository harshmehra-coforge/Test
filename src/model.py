"""Model schema/model for ML models."""
from pydantic import BaseModel
from typing import Optional, Dict

class Model(BaseModel):
    model_id: str
    user_id: str
    source_uri: str
    type: str
    params: Dict[str, str]
    artifacts_ref: Optional[str]
    status: str
