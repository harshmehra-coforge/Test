"""SQLAlchemy entities for quotes, policies, and FNOL claims."""
from datetime import date, datetime, timezone
from decimal import Decimal
from sqlalchemy import Date, DateTime, ForeignKey, Integer, Numeric, String, Index
from sqlalchemy.orm import Mapped, mapped_column, relationship
from .db import Base

class Quote(Base):
    """Persisted indicative premium and rating inputs."""
    __tablename__ = "quotes"
    quote_id: Mapped[str] = mapped_column(String(36), primary_key=True)
    driver_age: Mapped[int] = mapped_column(Integer, nullable=False)
    vehicle_type: Mapped[str] = mapped_column(String(20), nullable=False)
    premium: Mapped[Decimal] = mapped_column(Numeric(10, 2), nullable=False)
    rating_version: Mapped[str] = mapped_column(String(20), nullable=False)
    created_at: Mapped[datetime] = mapped_column(DateTime, default=lambda: datetime.now(timezone.utc), nullable=False)
    policy: Mapped["Policy | None"] = relationship(back_populates="quote")

class Policy(Base):
    """Bound policy with copied rating context and inclusive coverage dates."""
    __tablename__ = "policies"
    policy_id: Mapped[str] = mapped_column(String(36), primary_key=True)
    quote_id: Mapped[str] = mapped_column(ForeignKey("quotes.quote_id"), unique=True, nullable=False)
    driver_age: Mapped[int] = mapped_column(Integer, nullable=False)
    vehicle_type: Mapped[str] = mapped_column(String(20), nullable=False)
    premium: Mapped[Decimal] = mapped_column(Numeric(10, 2), nullable=False)
    coverage_start: Mapped[date] = mapped_column(Date, nullable=False)
    coverage_end: Mapped[date] = mapped_column(Date, nullable=False)
    status: Mapped[str] = mapped_column(String(20), nullable=False, default="ACTIVE")
    created_at: Mapped[datetime] = mapped_column(DateTime, default=lambda: datetime.now(timezone.utc), nullable=False)
    quote: Mapped[Quote] = relationship(back_populates="policy")
    claims: Mapped[list["Claim"]] = relationship(back_populates="policy")
    __table_args__ = (Index("ix_policy_status_end", "status", "coverage_end"),)

class Claim(Base):
    """Basic FNOL record linked to an existing policy."""
    __tablename__ = "claims"
    claim_id: Mapped[str] = mapped_column(String(36), primary_key=True)
    policy_id: Mapped[str] = mapped_column(ForeignKey("policies.policy_id"), nullable=False)
    loss_date: Mapped[date] = mapped_column(Date, nullable=False)
    report_date: Mapped[date] = mapped_column(Date, nullable=False)
    loss_description: Mapped[str] = mapped_column(String(2000), nullable=False)
    status: Mapped[str] = mapped_column(String(20), nullable=False, default="SUBMITTED")
    created_at: Mapped[datetime] = mapped_column(DateTime, default=lambda: datetime.now(timezone.utc), nullable=False)
    policy: Mapped[Policy] = relationship(back_populates="claims")
