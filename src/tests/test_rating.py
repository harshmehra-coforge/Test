"""Mandatory unit tests for v1 premium rules."""
from decimal import Decimal
from app.services.rating import calculate_premium

def test_standard_sedan_rating():
    assert calculate_premium(30, "sedan") == Decimal("600.00")

def test_young_suv_rating():
    assert calculate_premium(20, "suv") == Decimal("1125.00")

def test_senior_sports_rating():
    assert calculate_premium(70, "sports") == Decimal("1200.00")
