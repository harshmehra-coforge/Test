"""SQLAlchemy engine and session helpers for local SQLite persistence."""
from collections.abc import Iterator
from contextlib import contextmanager
from sqlalchemy import create_engine, event, text
from sqlalchemy.orm import DeclarativeBase, Session

class Base(DeclarativeBase):
    """Declarative base for domain entities."""

_engine = None

def init_db(database_url: str) -> None:
    """Create the engine and schema for the demo.

    # TODO: replace create_all with Alembic migrations before production.
    """
    global _engine
    _engine = create_engine(database_url, future=True)
    if database_url.startswith("sqlite"):
        event.listen(_engine, "connect", lambda conn, _: conn.execute("PRAGMA foreign_keys=ON"))
    from . import models  # noqa: F401
    Base.metadata.create_all(_engine)

@contextmanager
def session_scope() -> Iterator[Session]:
    """Yield a transactional session and roll back failed operations."""
    if _engine is None:
        raise RuntimeError("Database has not been initialized")
    session = Session(_engine)
    try:
        yield session
        session.commit()
    except Exception:
        session.rollback()
        raise
    finally:
        session.close()

def database_ready() -> bool:
    """Return whether a simple database query succeeds."""
    try:
        with session_scope() as session:
            session.execute(text("SELECT 1"))
        return True
    except Exception:
        return False
