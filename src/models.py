"""Typed calculator contracts."""
from enum import StrEnum
from pydantic import BaseModel, ConfigDict

class Operation(StrEnum):
    """Supported arithmetic operations."""
    ADD = "add"
    SUBTRACT = "subtract"
    MULTIPLY = "multiply"
    DIVIDE = "divide"

class CalculationRequest(BaseModel):
    """Input operands and operation."""
    model_config = ConfigDict(extra="forbid")
    left: float
    right: float
    operation: Operation

class CalculationResponse(BaseModel):
    """Calculation result."""
    left: float
    right: float
    operation: Operation
    result: float
