"""Domain exceptions and stable API error metadata."""
class DomainError(Exception):
    """Base error carrying an API-safe code and HTTP status."""
    def __init__(self, code: str, message: str, status: int = 400, field: str | None = None) -> None:
        super().__init__(message)
        self.code, self.message, self.status, self.field = code, message, status, field

class NotFoundError(DomainError):
    """Requested resource does not exist."""

class ConflictError(DomainError):
    """Requested state transition conflicts with stored state."""
