"""Local application entrypoint."""
from app import create_app
from app.config import Settings

app = create_app()

if __name__ == "__main__":
    settings = Settings()
    app.run(host=settings.host, port=settings.port, debug=settings.debug)
