"""
Main entrypoint for the BizWeb Flight-Pass microservices application.
Initializes FastAPI app, registers routers, and configures core services.
"""
from fastapi import FastAPI
from core.config import settings
from core.errors import add_error_handlers
from api.routes import purchase, credit, booking, admin, notification, audit

app = FastAPI(title="BizWeb Flight-Pass API")

# Register routers
app.include_router(purchase.router)
app.include_router(credit.router)
app.include_router(booking.router)
app.include_router(admin.router)
app.include_router(notification.router)
app.include_router(audit.router)

# Add error handlers
add_error_handlers(app)

# TODO: Add startup/shutdown events, DI wiring, event bus, etc.
