"""
/config endpoint routes. Admin/config management API (RBAC enforced).
"""
from fastapi import APIRouter, Depends, HTTPException
from src.services.config_manager import ConfigManager
from src.models.config import ConfigUpdateRequest, ConfigResponse

router = APIRouter(prefix="/config", tags=["config"])

@router.get("/", response_model=ConfigResponse)
async def get_config(
    manager: ConfigManager = Depends(ConfigManager)
):
    """Get current config state."""
    # TODO: Fetch config from Cosmos DB
    raise NotImplementedError

@router.post("/update", response_model=ConfigResponse)
async def update_config(
    update: ConfigUpdateRequest,
    manager: ConfigManager = Depends(ConfigManager)
):
    """Update config (admin only, RBAC enforced)."""
    # TODO: Validate, update config, audit log
    raise NotImplementedError
