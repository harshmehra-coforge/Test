"""Flask application factory and dependency wiring."""
from flask import Flask
from .config import Settings
from .db import init_db
from .api.routes import api
from .web.routes import web


def create_app(settings: Settings | None = None) -> Flask:
    """Create a configured application instance."""
    app = Flask(__name__, template_folder="web/templates", static_folder="web/static")
    app.config.from_mapping((settings or Settings()).as_flask_config())
    init_db(app.config["DATABASE_URL"])
    app.register_blueprint(api, url_prefix="/api/v1")
    app.register_blueprint(web)
    return app
