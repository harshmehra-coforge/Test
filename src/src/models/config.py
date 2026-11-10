"""
Config, ConfigUpdateRequest, and ConfigResponse models for configuration management.
"""
from pydantic import BaseModel
from typing import List, Optional

class Config(BaseModel):
    retry_max_attempts: int
    retry_interval_sec: int
    supported_currencies: List[str]
    gateways_enabled: List[str]
    # TODO: Add additional config fields as needed

class ConfigUpdateRequest(BaseModel):
    retry_max_attempts: Optional[int] = None
    retry_interval_sec: Optional[int] = None
    supported_currencies: Optional[List[str]] = None
    gateways_enabled: Optional[List[str]] = None
    # TODO: Add additional update fields as needed

class ConfigResponse(BaseModel):
    config: Config
    updated_at: str
