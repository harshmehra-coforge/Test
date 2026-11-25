"""Required deterministic rating-v1 unit checks."""
from decimal import Decimal
import pytest
from app.rating import calculate_premium


@pytest.mark.parametrize("age,vehicle,expected", [(22, "sedan", Decimal("1350.00")), (40, "suv", Decimal("1100.00")), (70, "truck", Decimal("1440.00"))])
def test_required_rating_examples(age: int, vehicle: str, expected: Decimal) -> None:
    """Verify the three TSD-approved premium examples."""
    assert calculate_premium(age, vehicle).premium == expected


def test_underage_driver_is_rejected() -> None:
    """Verify the underage edge case has a stable code."""
    with pytest.raises(Exception, match="Driver must"):
        calculate_premium(17, "sedan")
