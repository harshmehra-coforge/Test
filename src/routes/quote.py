"""
/quote endpoint: Calculate auto premium.
"""
from flask import Blueprint, request, jsonify

quote_bp = Blueprint('quote', __name__)

@quote_bp.route('/quote', methods=['POST'])
def calculate_quote():
    """Calculate premium based on driver age and vehicle type."""
    # TODO: Parse input, call rating_service, return result
    return jsonify({"premium": 0.0, "eligible": False, "message": "TODO"})
