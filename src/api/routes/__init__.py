"""
Route package init and blueprint registration.
"""
from flask import Flask
from .quote import quote_bp
from .policy import policy_bp
from .claim import claim_bp

def register_blueprints(app: Flask):
    """Register all API blueprints."""
    app.register_blueprint(quote_bp)
    app.register_blueprint(policy_bp)
    app.register_blueprint(claim_bp)
