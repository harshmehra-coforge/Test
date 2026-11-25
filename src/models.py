"""
SQLAlchemy models for Policy and Claim.
"""
from flask_sqlalchemy import SQLAlchemy
from datetime import datetime, date

db = SQLAlchemy()

class Policy(db.Model):
    __tablename__ = 'policies'
    id = db.Column(db.Integer, primary_key=True)
    policy_number = db.Column(db.String, unique=True, nullable=False)
    insured_name = db.Column(db.String, nullable=False)
    driver_age = db.Column(db.Integer, nullable=False)
    vehicle_type = db.Column(db.String, nullable=False)
    premium = db.Column(db.Float, nullable=False)
    effective_date = db.Column(db.Date, nullable=False)
    expiry_date = db.Column(db.Date, nullable=False)
    status = db.Column(db.String, nullable=False)

class Claim(db.Model):
    __tablename__ = 'claims'
    id = db.Column(db.Integer, primary_key=True)
    claim_number = db.Column(db.String, unique=True, nullable=False)
    policy_number = db.Column(db.String, db.ForeignKey('policies.policy_number'), nullable=False)
    incident_date = db.Column(db.Date, nullable=False)
    description = db.Column(db.String, nullable=False)
    claim_status = db.Column(db.String, nullable=False)
    created_at = db.Column(db.DateTime, default=datetime.utcnow)
