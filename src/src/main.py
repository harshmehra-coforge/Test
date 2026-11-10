"""
FastAPI app factory and API gateway wiring for Flight-Pass module.
"""
from fastapi import FastAPI
from core.config import get_settings
from api.routes import flightpass, booking, ledger, user, admin


def create_app() -> FastAPI:
    """App factory for the Flight-Pass API gateway."""
    app = FastAPI(title="BizWeb Flight-Pass API")
    app.include_router(flightpass.router)
    app.include_router(booking.router)
    app.include_router(ledger.router)
    app.include_router(user.router)
    app.include_router(admin.router)
    return app

app = create_app()
