"""Job management endpoints."""
from fastapi import APIRouter

router = APIRouter(prefix="/jobs", tags=["jobs"])

@router.post("/")
async def submit_job():
    """Submit a new fine-tuning/build/deploy job."""
    # TODO: Validate and enqueue job
    raise NotImplementedError

@router.get("/{job_id}")
async def get_job(job_id: str):
    """Get job status/details."""
    # TODO: Fetch job status from DB
    raise NotImplementedError
