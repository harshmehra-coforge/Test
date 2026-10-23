"""
Entrypoint and FastAPI app factory.
"""
from fastapi import FastAPI
from src.api import feedback, resubmission, notification, audit

app = FastAPI(title="Rejection Handling & Resubmission API")

# Register routers
app.include_router(feedback.router, prefix="/feedback", tags=["Feedback"])
app.include_router(resubmission.router, prefix="/resubmission", tags=["Resubmission"])
app.include_router(notification.router, prefix="/notification", tags=["Notification"])
app.include_router(audit.router, prefix="/audit", tags=["Audit"])

# TODO: Add startup/shutdown events, health checks, etc.
