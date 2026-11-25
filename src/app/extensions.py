"""Shared Flask extensions kept separate for dependency-injection-friendly setup."""
from flask_sqlalchemy import SQLAlchemy

from .models import BaseModel

db: SQLAlchemy = SQLAlchemy(model_class=BaseModel)
