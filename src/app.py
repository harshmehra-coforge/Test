"""
Entrypoint for the Auto Insurance Quote & FNOL Claim App.
Initializes Flask app, config, DB, and registers routes.
"""
from flask import Flask
from config.settings import get_config
from models import init_db
from api.routes import register_blueprints

def create_app() -> Flask:
    """App factory."""
    app = Flask(__name__)
    app.config.from_object(get_config())
    init_db(app)
    register_blueprints(app)
    return app

if __name__ == "__main__":
    app = create_app()
    app.run(host="0.0.0.0", port=5000, debug=True)
