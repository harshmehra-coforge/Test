"""Deterministic quote and premium-rating service."""
from decimal import Decimal, ROUND_HALF_UP
from uuid import uuid4
from . import __name__ as _services_package
from ..errors import DomainError
from ..models import Quote
from ..repositories import Repository

BASE_PREMIUMS = {"sedan": Decimal("600.00"), "suv": Decimal("750.00"), "sports": Decimal("1000.00")}
RATING_VERSION = "v1"

def calculate_premium(driver_age: int, vehicle_type: str) -> Decimal:
    """Calculate a rounded premium from the TSD's v1 rules."""
    # TODO: replace demonstration values only through an approved versioned table.
    if not isinstance(driver_age, int) or not 0 <= driver_age <= 120:
        raise DomainError("INVALID_INPUT", "Driver age must be an integer from 0 to 120.", field="driver_age")
    if driver_age < 18:
        raise DomainError("DRIVER_UNDERAGE", "Driver must be at least 18 years old.", 422, "driver_age")
    if vehicle_type not in BASE_PREMIUMS:
        raise DomainError("UNSUPPORTED_VEHICLE_TYPE", "Vehicle type is not supported.", 422, "vehicle_type")
    multiplier = Decimal("1.50") if driver_age <= 24 else Decimal("1.20") if driver_age >= 65 else Decimal("1.00")
    return (BASE_PREMIUMS[vehicle_type] * multiplier).quantize(Decimal("0.01"), rounding=ROUND_HALF_UP)

def create_quote(repo: Repository, driver_age: int, vehicle_type: str) -> Quote:
    """Validate and persist an eligible quote."""
    premium = calculate_premium(driver_age, vehicle_type)
    return repo.add(Quote(quote_id=str(uuid4()), driver_age=driver_age, vehicle_type=vehicle_type, premium=premium, rating_version=RATING_VERSION))
