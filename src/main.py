"""FastAPI app factory and startup wiring for Flight-Pass microservices."""
from fastapi import FastAPI
from src.api.routes import bundles, credits, users, booking, admin, reporting


def create_app() -> FastAPI:
    """Create and configure FastAPI app."""
    app = FastAPI(title="Flight-Pass Service")
    # Register routers
    app.include_router(bundles.router)
    app.include_router(credits.router)
    app.include_router(users.router)
    app.include_router(booking.router)
    app.include_router(admin.router)
    app.include_router(reporting.router)
    # TODO: Add startup/shutdown event handlers, middleware, DI wiring
    return app

app = create_app()
