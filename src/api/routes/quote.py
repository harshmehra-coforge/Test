"""
/quote endpoint: Calculate premium for driver age and vehicle type.
"""
from flask import Blueprint, request, jsonify

quote_bp = Blueprint('quote', __name__, url_prefix='/quote')

@quote_bp.route('', methods=['POST'])
def get_quote():
    """Calculate and return premium."""
    # TODO: Parse input, call rating service, return response
    return jsonify({"message": "TODO: Implement quote endpoint"}), 501
