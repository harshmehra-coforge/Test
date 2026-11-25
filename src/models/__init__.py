"""
Model package init. Imports and DB init for Policy and Claim models.
"""
from flask_sqlalchemy import SQLAlchemy

db = SQLAlchemy()

def init_db(app):
    """Initialize DB with app context."""
    db.init_app(app)
    # TODO: Create tables if not exist
