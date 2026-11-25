"""
Claim ORM model.
"""
from . import db
from datetime import date

class Claim(db.Model):
    """FNOL Claim model."""
    __tablename__ = "claims"
    id = db.Column(db.Integer, primary_key=True)
    claim_number = db.Column(db.String, unique=True, nullable=False)
    policy_id = db.Column(db.Integer, db.ForeignKey('policies.id'), nullable=False)
    loss_date = db.Column(db.Date, nullable=False)
    description = db.Column(db.String, nullable=False)
    status = db.Column(db.String, nullable=False)  # 'open', 'closed', 'rejected'
    created_at = db.Column(db.Date, nullable=False)

    # TODO: Add relationship to Policy, status transitions, etc.
