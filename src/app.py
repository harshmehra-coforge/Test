"""
Entrypoint for the Auto Insurance Quote & FNOL Claim App.
Initializes Flask app, config, and registers Blueprints.
"""
from flask import Flask
from config import Config
from models import db
from routes import quote_bp, policy_bp, claim_bp


def create_app():
    app = Flask(__name__)
    app.config.from_object(Config)
    db.init_app(app)

    app.register_blueprint(quote_bp)
    app.register_blueprint(policy_bp)
    app.register_blueprint(claim_bp)

    return app

if __name__ == "__main__":
    app = create_app()
    app.run(debug=True)
