"""Error handling utilities."""
from fastapi import Request, HTTPException

# TODO: Implement custom exception handlers, error response formatting

def handle_exception(request: Request, exc: Exception):
    """Global exception handler."""
    raise NotImplementedError
