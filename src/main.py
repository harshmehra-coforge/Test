"""FastAPI application factory."""
from fastapi import FastAPI
from calculator.api.routes import router
from calculator.config import Settings, get_settings

def create_app(settings: Settings | None = None) -> FastAPI:
    """Create and configure the application."""
    app_settings = settings or get_settings()
    app = FastAPI(title=app_settings.app_name, debug=app_settings.debug)
    app.include_router(router)
    return app

app = create_app()
