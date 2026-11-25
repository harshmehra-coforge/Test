"""Pure rating-v1 calculation and stable domain validation errors."""
from dataclasses import dataclass
from decimal import Decimal, ROUND_HALF_UP

BASE_PREMIUMS: dict[str, Decimal] = {"sedan": Decimal("900"), "suv": Decimal("1100"), "truck": Decimal("1200"), "motorcycle": Decimal("800")}
RATING_VERSION = "rating-v1"


class DomainError(ValueError):
    """Expected domain validation failure with stable API code and field."""
    def __init__(self, code: str, message: str, field: str | None = None) -> None:
        super().__init__(message)
        self.code, self.message, self.field = code, message, field


@dataclass(frozen=True)
class RatingResult:
    """Rating output retained by quote and policy flows."""
    premium: Decimal
    currency: str
    rating_version: str
    driver_age: int
    vehicle_type: str


def calculate_premium(driver_age: int, vehicle_type: str) -> RatingResult:
    """Calculate the deterministic MVP premium; no database or HTTP side effects."""
    if not isinstance(driver_age, int) or isinstance(driver_age, bool) or not 0 <= driver_age <= 120:
        raise DomainError("INVALID_AGE", "Driver age must be an integer from 0 through 120.", "driver_age")
    if driver_age < 18:
        raise DomainError("UNDERAGE_DRIVER", "Driver must be at least 18 years old.", "driver_age")
    if vehicle_type not in BASE_PREMIUMS:
        raise DomainError("INVALID_VEHICLE_TYPE", "Vehicle type is not supported.", "vehicle_type")
    factor = Decimal("1.50") if driver_age <= 24 else Decimal("1.20") if driver_age >= 65 else Decimal("1.00")
    premium = (BASE_PREMIUMS[vehicle_type] * factor).quantize(Decimal("0.01"), rounding=ROUND_HALF_UP)
    return RatingResult(premium, "USD", RATING_VERSION, driver_age, vehicle_type)
