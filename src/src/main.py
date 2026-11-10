"""
Entrypoint for the Payment Gateway Integration Service.
Initializes FastAPI app, registers routers, and configures startup/shutdown events.
"""
from fastapi import FastAPI
from src.core.config import get_settings
from src.api.routes import payments, status, config


def create_app() -> FastAPI:
    """App factory for FastAPI."""
    app = FastAPI(title="Payment Gateway Integration Service")

    # Register routers
    app.include_router(payments.router)
    app.include_router(status.router)
    app.include_router(config.router)

    # TODO: Add startup/shutdown event handlers (DB, telemetry, etc)
    return app


app = create_app()
