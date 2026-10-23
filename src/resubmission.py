"""
FastAPI router for resubmission endpoints.
"""
from fastapi import APIRouter, Depends, HTTPException
from src.service.resubmission_service import ResubmissionService
from src.models.story import Story

router = APIRouter()

@router.post("/{story_id}", response_model=Story)
async def resubmit_story(story_id: str, updated_data: dict, service: ResubmissionService = Depends()):
    """Resubmit a story after rejection."""
    # TODO: Wire up service and implement logic
    raise NotImplementedError
