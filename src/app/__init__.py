"""Application factory and SQLite initialization for the auto insurance MVP."""
from flask import Flask
from sqlalchemy import event
from sqlalchemy.engine import Engine

from .config import Settings
from .extensions import db


def create_app(settings: Settings | None = None) -> Flask:
    """Create and configure the Flask application."""
    app = Flask(__name__, template_folder="templates", static_folder="static")
    app.config.from_mapping((settings or Settings()).as_flask_config())
    db.init_app(app)
    from .api import api_bp
    app.register_blueprint(api_bp)
    with app.app_context():
        db.create_all()
    return app


@event.listens_for(Engine, "connect")
def _configure_sqlite(dbapi_connection: object, _connection_record: object) -> None:
    """Enable SQLite integrity and local read-concurrency settings."""
    # TODO: apply WAL pragmas for SQLite connections while preserving portability.
    cursor = dbapi_connection.cursor()
    cursor.execute("PRAGMA foreign_keys=ON")
    cursor.execute("PRAGMA journal_mode=WAL")
    cursor.close()
