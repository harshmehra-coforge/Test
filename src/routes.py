"""FastAPI routes."""
from fastapi import APIRouter, Depends, HTTPException, status
from calculator.models import CalculationRequest, CalculationResponse
from calculator.service import CalculationError, CalculatorService
router = APIRouter()

def get_calculator_service() -> CalculatorService:
    """Provide calculator service dependency."""
    return CalculatorService()

@router.get("/health", tags=["health"])
def health_check() -> dict[str, str]:
    """Return liveness status."""
    return {"status": "ok"}

@router.post("/v1/calculate", response_model=CalculationResponse, tags=["calculator"])
def calculate(request: CalculationRequest, service: CalculatorService = Depends(get_calculator_service)) -> CalculationResponse:
    """Calculate an arithmetic operation."""
    try:
        return service.calculate(request)
    except CalculationError as exc:
        raise HTTPException(status_code=status.HTTP_422_UNPROCESSABLE_ENTITY, detail=str(exc)) from exc
