"""SQLAlchemy persistence models for policies and FNOL claims."""
from datetime import date, datetime, timezone
from decimal import Decimal
from typing import Any
from uuid import uuid4

from sqlalchemy import Date, DateTime, ForeignKey, Numeric, String, Text
from sqlalchemy.orm import DeclarativeBase, Mapped, mapped_column, relationship


class BaseModel(DeclarativeBase):
    """Declarative base for persistence models."""


class Policy(BaseModel):
    """Bound policy record retaining the rating basis and coverage dates."""
    __tablename__ = "policies"
    policy_id: Mapped[str] = mapped_column(String(36), primary_key=True, default=lambda: str(uuid4()))
    driver_age: Mapped[int] = mapped_column(nullable=False)
    vehicle_type: Mapped[str] = mapped_column(String(20), nullable=False)
    premium: Mapped[Decimal] = mapped_column(Numeric(10, 2), nullable=False)
    rating_version: Mapped[str] = mapped_column(String(20), nullable=False)
    effective_date: Mapped[date] = mapped_column(Date, nullable=False)
    expiration_date: Mapped[date] = mapped_column(Date, nullable=False)
    status: Mapped[str] = mapped_column(String(12), nullable=False, default="ACTIVE")
    created_at: Mapped[datetime] = mapped_column(DateTime, default=lambda: datetime.now(timezone.utc), nullable=False)
    claims: Mapped[list["Claim"]] = relationship(back_populates="policy")


class Claim(BaseModel):
    """Basic FNOL claim linked to one policy."""
    __tablename__ = "claims"
    claim_id: Mapped[str] = mapped_column(String(36), primary_key=True, default=lambda: str(uuid4()))
    policy_id: Mapped[str] = mapped_column(ForeignKey("policies.policy_id"), nullable=False, index=True)
    loss_date: Mapped[date] = mapped_column(Date, nullable=False)
    reported_date: Mapped[date] = mapped_column(Date, nullable=False)
    loss_type: Mapped[str] = mapped_column(String(50), nullable=False)
    description: Mapped[str] = mapped_column(Text, nullable=False)
    status: Mapped[str] = mapped_column(String(20), nullable=False, default="FNOL_SUBMITTED")
    created_at: Mapped[datetime] = mapped_column(DateTime, default=lambda: datetime.now(timezone.utc), nullable=False)
    policy: Mapped[Policy] = relationship(back_populates="claims")


def serialize_model(model: Any) -> dict[str, Any]:
    """Serialize a Policy or Claim to JSON-safe primitive values."""
    # TODO: replace generic serialization with explicit response schemas if contracts expand.
    result = {column.name: getattr(model, column.name) for column in model.__table__.columns}
    return {key: value.isoformat() if isinstance(value, (date, datetime)) else str(value) if isinstance(value, Decimal) else value
            for key, value in result.items()}
