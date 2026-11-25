"""
Policy ORM model.
"""
from . import db
from datetime import date

class Policy(db.Model):
    """Insurance Policy model."""
    __tablename__ = "policies"
    id = db.Column(db.Integer, primary_key=True)
    policy_number = db.Column(db.String, unique=True, nullable=False)
    insured_name = db.Column(db.String, nullable=False)
    start_date = db.Column(db.Date, nullable=False)
    end_date = db.Column(db.Date, nullable=False)
    driver_age = db.Column(db.Integer, nullable=False)
    vehicle_type = db.Column(db.String, nullable=False)
    premium = db.Column(db.Float, nullable=False)
    status = db.Column(db.String, nullable=False)  # 'active', 'expired'

    # TODO: Add methods for status calculation, etc.
