"""
FastAPI entrypoint for Discount Approval Microservice
"""
from fastapi import FastAPI
from src.api.routes import router

app = FastAPI(title="Discount Approval Microservice")
app.include_router(router)
