"""FastAPI app factory and startup wiring."""
from fastapi import FastAPI
from src.core.config import get_settings
from src.api.routes import dashboard, jobs, models, deploy

app = FastAPI(title="LoRA/QLoRA Fine-Tuning Platform")

# Register routers
app.include_router(dashboard.router)
app.include_router(jobs.router)
app.include_router(models.router)
app.include_router(deploy.router)

@app.on_event("startup")
def startup_event():
    """Startup logic: load config, connect to Azure, etc."""
    # TODO: Initialize connections, logging, etc.
    pass
