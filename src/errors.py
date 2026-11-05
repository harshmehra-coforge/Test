"""
Error handling utilities for BizWeb Flight-Pass.
Defines custom exceptions and FastAPI exception handlers.
"""
from fastapi import FastAPI, Request
from fastapi.responses import JSONResponse

class BizWebError(Exception):
    """Base exception for BizWeb Flight-Pass."""
    pass

# TODO: Define more specific exceptions as needed

def add_error_handlers(app: FastAPI):
    @app.exception_handler(BizWebError)
    async def bizweb_error_handler(request: Request, exc: BizWebError):
        return JSONResponse(status_code=400, content={"detail": str(exc)})
    # TODO: Add more handlers for other exceptions
